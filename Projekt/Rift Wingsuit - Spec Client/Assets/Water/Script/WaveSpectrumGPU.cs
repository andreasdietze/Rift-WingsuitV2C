using UnityEngine;
using System;
using System.Collections;

namespace OceanSurfaceEffects
{
    /// <summary>
    /// 
    /// Generates a wave spectrum using the formula in the follow research paper.
    /// Can the found with a bit of googling.
    /// 
    /// WAVES SPECTRUM
    /// using "A unified directional spectrum for long and short wind-driven waves"
    /// T. Elfouhaily, B. Chapron, K. Katsaros, D. Vandemark
    /// Journal of Geophysical Research vol 102, p781-796, 1997
    /// 
    /// </summary>
    public class WaveSpectrumGPU
    {
        //CONST DONT CHANGE
        const float WAVE_CM = 0.23f;	// Eq 59
        const float WAVE_KM = 370.0f;	// Eq 59

        /// <summary>
        /// The current buffer index.
        /// Flips between 0 and 1 each frame. 
        /// </summary>
        int m_idx = 0;

        /// <summary>
        /// This is the fourier transform size, must pow2 number. 
        /// </summary>
        int m_size = 128;
        float m_fsize;

        /// <summary>
        /// A higher wind speed gives greater swell to the waves.
        /// </summary>
        float m_windSpeed = 8.0f;

        /// <summary>
        /// Scales the height of the waves.
        /// </summary>
        float m_waveAmp = 1.0f;

        /// <summary>
        /// A lower number means the waves last longer and 
        /// will build up larger waves.
        /// </summary>
        float m_omega = 0.84f;

		/// <summary>
		/// Used for uv offset. 
		/// </summary>
		Vector4 m_offset;

        /// <summary>
        /// The waves are made up of 4 layers of heights
        /// at different wave lengths. These grid sizes 
        /// are basically the wave length for each layer.
        /// </summary>
        public Vector4 gridSizes { get { return m_gridSizes; } }
        Vector4 m_gridSizes = new Vector4(5488, 392, 28, 2);
        Vector4 m_inverseGridSizes;

        /// <summary>
        /// Number of mipmap levels in the height maps.
        /// </summary>
        public float mipMapLevels { get { return m_mipMapLevels; } }
        float m_mipMapLevels;

        /// <summary>
        /// Material to initilize the spectrum before fourier transform.
        /// </summary>
        Material m_initSpectrumMat;

        /// <summary>
        /// The fourier buffers and spectrum data.
        /// </summary>
        Texture2D m_spectrum01, m_spectrum23;
        RenderTexture[] m_fourierBuffer0, m_fourierBuffer1, m_fourierBuffer2;
        Texture2D m_WTable;

        /// <summary>
        /// The maps holding the height and slope data.
        /// </summary>
        public RenderTexture map0 { get { return m_map0; } }
        public RenderTexture map1 { get { return m_map1; } }
        public RenderTexture map2 { get { return m_map2; } }
        RenderTexture m_map0, m_map1, m_map2;

        /// <summary>
        /// Fourier object to do the fourier transform.
        /// </summary>
        FourierGPU m_fourier;

        public WaveSpectrumGPU(int size, float windSpeed, float waveAmp, float omega, int ansio, Vector4 gridSzes)
        {

            //Fourier size can not be greater than 256 because the
            //FourierGPU object uses a 8 bit texture for the 
            //butterfly look up table. This limitation could be
            //removed in theory if a look up table with more bits is used.
            if (size > 256)
            {
                Debug.Log("Fourier grid size must not be greater than 256, changing to 256");
                size = 256;
            }

            if (!Mathf.IsPowerOfTwo(size))
            {
                Debug.Log("Fourier grid size must be pow2 number, changing to nearest pow2 number");
                size = Mathf.NextPowerOfTwo(size);
            }

            m_size = size;
            m_waveAmp = waveAmp;
            m_windSpeed = windSpeed;
            m_omega = omega;
            m_gridSizes = gridSzes;

            m_fsize = (float)m_size;
            m_mipMapLevels = Mathf.Log(m_fsize) / Mathf.Log(2.0f);
            m_offset = new Vector4(1.0f + 0.5f / m_fsize, 1.0f + 0.5f / m_fsize, 0, 0);

            float factor = 2.0f * Mathf.PI * m_fsize;
            m_inverseGridSizes = new Vector4(factor / m_gridSizes.x, factor / m_gridSizes.y, factor / m_gridSizes.z, factor / m_gridSizes.w);

            m_fourier = new FourierGPU(m_size);

            CreateRenderTextures(ansio);

            //Load the init spectrum shader. Must be in the resources folder.
            Shader initSpectrumShader = Shader.Find("Ocean/InitSpectrum");

            if (initSpectrumShader == null)
                throw new InvalidOperationException("Could not find shader Ocean/InitSpectrum");

            m_initSpectrumMat = new Material(initSpectrumShader);

        }

        /// <summary>
        /// Must call release before object is destroyed else the
        /// data in the render textures will be leaked.
        /// </summary>
        public void Release()
        {
            m_map0.Release();
            m_map1.Release();
            m_map2.Release();

            for (int i = 0; i < 2; i++)
            {
                m_fourierBuffer0[i].Release();
                m_fourierBuffer1[i].Release();
                m_fourierBuffer2[i].Release();
            }
        }

        /// <summary>
        /// Creates all the data needed to generate the waves.
        /// This is not called in the constructor because greater control
        /// over when exactly this data is created is needed.
        /// </summary>
        public void Init()
        {

            GenerateWavesSpectrum();
            CreateWTable();

            m_initSpectrumMat.SetTexture("_Spectrum01", m_spectrum01);
            m_initSpectrumMat.SetTexture("_Spectrum23", m_spectrum23);
            m_initSpectrumMat.SetTexture("_WTable", m_WTable);

        }

        /// <summary>
        /// Create all the textures need to hold the data.
        /// </summary>
        void CreateRenderTextures(int ansio)
        {
            RenderTextureFormat mapFormat = RenderTextureFormat.ARGBHalf;
            RenderTextureFormat format = RenderTextureFormat.ARGBFloat;

            //These texture hold the actual data use in the ocean renderer
            m_map0 = new RenderTexture(m_size, m_size, 0, mapFormat);
            m_map0.filterMode = FilterMode.Trilinear;
            m_map0.wrapMode = TextureWrapMode.Repeat;
            m_map0.anisoLevel = ansio;
            m_map0.useMipMap = true;
            m_map0.Create();

            m_map1 = new RenderTexture(m_size, m_size, 0, mapFormat);
            m_map1.filterMode = FilterMode.Trilinear;
            m_map1.wrapMode = TextureWrapMode.Repeat;
            m_map1.anisoLevel = ansio;
            m_map1.useMipMap = true;
            m_map1.Create();

            m_map2 = new RenderTexture(m_size, m_size, 0, mapFormat);
            m_map2.filterMode = FilterMode.Trilinear;
            m_map2.wrapMode = TextureWrapMode.Repeat;
            m_map2.anisoLevel = ansio;
            m_map2.useMipMap = true;
            m_map2.Create();

            //These textures hold the specturm the fourier transform is performed on
			m_spectrum01 = new Texture2D(m_size, m_size, TextureFormat.RGBAFloat, false);
            m_spectrum01.filterMode = FilterMode.Point;
            m_spectrum01.wrapMode = TextureWrapMode.Repeat;

			m_spectrum23 = new Texture2D(m_size, m_size, TextureFormat.RGBAFloat, false);
            m_spectrum23.filterMode = FilterMode.Point;
            m_spectrum23.wrapMode = TextureWrapMode.Repeat;

			m_WTable = new Texture2D(m_size, m_size, TextureFormat.RGBAFloat, false);
            m_WTable.filterMode = FilterMode.Point;
            m_WTable.wrapMode = TextureWrapMode.Clamp;

            //These textures are used to perform the fourier transform
            m_fourierBuffer0 = new RenderTexture[2];
            m_fourierBuffer1 = new RenderTexture[2];
            m_fourierBuffer2 = new RenderTexture[2];

            CreateBuffer(m_fourierBuffer0, format);
            CreateBuffer(m_fourierBuffer1, format);
            CreateBuffer(m_fourierBuffer2, format);

        }

        void CreateBuffer(RenderTexture[] tex, RenderTextureFormat format)
        {
            for (int i = 0; i < 2; i++)
            {
                tex[i] = new RenderTexture(m_size, m_size, 0, format);
                tex[i].filterMode = FilterMode.Point;
                tex[i].wrapMode = TextureWrapMode.Clamp;
                tex[i].Create();
            }
        }

        float sqr(float x) { return x * x; }

        float omega(float k) { return Mathf.Sqrt(9.81f * k * (1.0f + sqr(k / WAVE_KM))); } // Eq 24

        /// <summary>
        /// I know this is a big chunk of ugly math but dont worry to much about what it all means
        /// It recreates a statistcally representative model of a wave spectrum in the frequency domain.
        /// </summary>
        float Spectrum(float kx, float ky)
        {

            float U10 = m_windSpeed;

            // phase speed
            float k = Mathf.Sqrt(kx * kx + ky * ky);
            float c = omega(k) / k;

            // spectral peak
            float kp = 9.81f * sqr(m_omega / U10); // after Eq 3
            float cp = omega(kp) / kp;

            // friction velocity
            float z0 = 3.7e-5f * sqr(U10) / 9.81f * Mathf.Pow(U10 / cp, 0.9f); // Eq 66
            float u_star = 0.41f * U10 / Mathf.Log(10.0f / z0); // Eq 60

            float Lpm = Mathf.Exp(-5.0f / 4.0f * sqr(kp / k)); // after Eq 3
            float gamma = (m_omega < 1.0f) ? 1.7f : 1.7f + 6.0f * Mathf.Log(m_omega); // after Eq 3 // log10 or log?
            float sigma = 0.08f * (1.0f + 4.0f / Mathf.Pow(m_omega, 3.0f)); // after Eq 3
            float Gamma = Mathf.Exp(-1.0f / (2.0f * sqr(sigma)) * sqr(Mathf.Sqrt(k / kp) - 1.0f));
            float Jp = Mathf.Pow(gamma, Gamma); // Eq 3
            float Fp = Lpm * Jp * Mathf.Exp(-m_omega / Mathf.Sqrt(10.0f) * (Mathf.Sqrt(k / kp) - 1.0f)); // Eq 32
            float alphap = 0.006f * Mathf.Sqrt(m_omega); // Eq 34
            float Bl = 0.5f * alphap * cp / c * Fp; // Eq 31

            float alpham = 0.01f * (u_star < WAVE_CM ? 1.0f + Mathf.Log(u_star / WAVE_CM) : 1.0f + 3.0f * Mathf.Log(u_star / WAVE_CM)); // Eq 44
            float Fm = Mathf.Exp(-0.25f * sqr(k / WAVE_KM - 1.0f)); // Eq 41
            float Bh = 0.5f * alpham * WAVE_CM / c * Fm * Lpm; // Eq 40 (fixed)

            float a0 = Mathf.Log(2.0f) / 4.0f;
            float ap = 4.0f;
            float am = 0.13f * u_star / WAVE_CM; // Eq 59
            float Delta = (float)System.Math.Tanh(a0 + ap * Mathf.Pow(c / cp, 2.5f) + am * Mathf.Pow(WAVE_CM / c, 2.5f)); // Eq 57

            float phi = Mathf.Atan2(ky, kx);

            if (kx < 0.0f) return 0.0f;

            Bl *= 2.0f;
            Bh *= 2.0f;

            return m_waveAmp * (Bl + Bh) * (1.0f + Delta * Mathf.Cos(2.0f * phi)) / (2.0f * Mathf.PI * sqr(sqr(k))); // Eq 67
        }

        Vector2 GetSpectrumSample(float i, float j, float lengthScale, float kMin)
        {
            float dk = 2.0f * Mathf.PI / lengthScale;
            float kx = i * dk;
            float ky = j * dk;
            Vector2 result = new Vector2(0.0f, 0.0f);

            float rnd = UnityEngine.Random.value;

            if (Mathf.Abs(kx) >= kMin || Mathf.Abs(ky) >= kMin)
            {
                float S = Spectrum(kx, ky);
                float h = Mathf.Sqrt(S / 2.0f) * dk;

                float phi = rnd * 2.0f * Mathf.PI;
                result.x = h * Mathf.Cos(phi);
                result.y = h * Mathf.Sin(phi);
            }

            return result;
        }

        /// <summary>
        /// Generates the wave spectrum based on the 
        /// settings wind speed, wave amp and wave age.
        /// If these values change this function must be called again.
        /// </summary>
        void GenerateWavesSpectrum()
        {

			Color[] spectrum01 = new Color[m_size * m_size];
			Color[] spectrum23 = new Color[m_size * m_size];

            int idx;
            float i;
            float j;
            Vector2 sample12XY;
            Vector2 sample12ZW;
            Vector2 sample34XY;
            Vector2 sample34ZW;

            UnityEngine.Random.seed = 0;

            for (int x = 0; x < m_size; x++)
            {
                for (int y = 0; y < m_size; y++)
                {
                    idx = x + y * m_size;
                    i = (x >= m_size / 2) ? (float)(x - m_size) : (float)x;
                    j = (y >= m_size / 2) ? (float)(y - m_size) : (float)y;

                    sample12XY = GetSpectrumSample(i, j, m_gridSizes.x, Mathf.PI / m_gridSizes.x);
                    sample12ZW = GetSpectrumSample(i, j, m_gridSizes.y, Mathf.PI * m_fsize / m_gridSizes.x);
                    sample34XY = GetSpectrumSample(i, j, m_gridSizes.z, Mathf.PI * m_fsize / m_gridSizes.y);
                    sample34ZW = GetSpectrumSample(i, j, m_gridSizes.w, Mathf.PI * m_fsize / m_gridSizes.z);

                    spectrum01[idx].r = sample12XY.x;
                    spectrum01[idx].g = sample12XY.y;
                    spectrum01[idx].b = sample12ZW.x;
                    spectrum01[idx].a = sample12ZW.y;

                    spectrum23[idx].r = sample34XY.x;
                    spectrum23[idx].g = sample34XY.y;
                    spectrum23[idx].b = sample34ZW.x;
                    spectrum23[idx].a = sample34ZW.y;
                }
            }

			m_spectrum01.SetPixels( spectrum01);
			m_spectrum23.SetPixels( spectrum23);

			m_spectrum01.Apply();
			m_spectrum23.Apply();

        }

        /// <summary>
        /// Some of the values needed in the InitWaveSpectrum function can be precomputed.
        /// If the grid sizes change this function must called again.
        /// </summary>
        void CreateWTable()
        {

            Vector2 uv, st;
            float k1, k2, k3, k4, w1, w2, w3, w4;

            Color[] table = new Color[m_size * m_size];

            for (int x = 0; x < m_size; x++)
            {
                for (int y = 0; y < m_size; y++)
                {
                    uv = new Vector2(x, y) / m_fsize;

                    st.x = uv.x > 0.5f ? uv.x - 1.0f : uv.x;
                    st.y = uv.y > 0.5f ? uv.y - 1.0f : uv.y;

                    k1 = (st * m_inverseGridSizes.x).magnitude;
                    k2 = (st * m_inverseGridSizes.y).magnitude;
                    k3 = (st * m_inverseGridSizes.z).magnitude;
                    k4 = (st * m_inverseGridSizes.w).magnitude;

                    w1 = Mathf.Sqrt(9.81f * k1 * (1.0f + k1 * k1 / (WAVE_KM * WAVE_KM)));
                    w2 = Mathf.Sqrt(9.81f * k2 * (1.0f + k2 * k2 / (WAVE_KM * WAVE_KM)));
                    w3 = Mathf.Sqrt(9.81f * k3 * (1.0f + k3 * k3 / (WAVE_KM * WAVE_KM)));
                    w4 = Mathf.Sqrt(9.81f * k4 * (1.0f + k4 * k4 / (WAVE_KM * WAVE_KM)));

                    table[(x + y * m_size)].r = w1;
                    table[(x + y * m_size)].g = w2;
                    table[(x + y * m_size)].b = w3;
                    table[(x + y * m_size)].a = w4;

                }
            }

			m_WTable.SetPixels(table);
			m_WTable.Apply();

        }

        /// <summary>
        /// Initilize the spectrum for a time period.
        /// </summary>
        void InitWaveSpectrum(float t)
        {
            m_initSpectrumMat.SetTexture("_Spectrum01", m_spectrum01);
            m_initSpectrumMat.SetTexture("_Spectrum23", m_spectrum23);
            m_initSpectrumMat.SetTexture("_WTable", m_WTable);
            m_initSpectrumMat.SetVector("_Offset", m_offset);
            m_initSpectrumMat.SetVector("_InverseGridSizes", m_inverseGridSizes);
            m_initSpectrumMat.SetFloat("_T", t);

            RenderTexture[] buffers = new RenderTexture[] { m_fourierBuffer0[1], m_fourierBuffer1[1], m_fourierBuffer2[1] };

            RTUtility.MultiTargetBlit(buffers, m_initSpectrumMat);
        }

        /// <summary>
        /// Calculate the wave heights at time period t.
        /// </summary>
        public void SimulateWaves(float t)
        {

            InitWaveSpectrum(t);

            m_idx = m_fourier.PeformFFT(m_fourierBuffer0, m_fourierBuffer1, m_fourierBuffer2);

            //Copy the contents of the completed fourier transform to the map textures.
            //You could just use the buffer textures (m_fourierBuffer0,1,2) to read from 
            //for the ocean shader but they need to have mipmaps and unity updates the mipmaps
            //every time the texture is renderer into. This impacts performance during 
            //fourier transform stage as mipmaps would be updated every pass and there is no way 
            //to disable and then enable mipmaps on render textures at time of writting.

            Graphics.Blit(m_fourierBuffer0[m_idx], m_map0);
            Graphics.Blit(m_fourierBuffer1[m_idx], m_map1);
            Graphics.Blit(m_fourierBuffer2[m_idx], m_map2);

        }

    }
	
}

