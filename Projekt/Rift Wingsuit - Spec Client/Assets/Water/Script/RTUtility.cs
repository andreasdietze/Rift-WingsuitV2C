using UnityEngine;
using System.Collections;


/// <summary>
/// A helper class for render textures related functions
/// </summary>
static public class RTUtility
{

	/// <summary>
	/// Replicates Unity's Graphics.Blit function.
	/// </summary>
	/// <param name="des">The destination render texture.</param>
	/// <param name="mat">The amterial to use</param>
	/// <param name="pass">Which pass of the materials shader to use.</param>
	static public void Blit(RenderTexture des, Material mat, int pass = 0)
    {
        RenderTexture oldRT = RenderTexture.active;
		
        Graphics.SetRenderTarget(des);

        GL.Clear(true, true, Color.clear);

        GL.PushMatrix();
        GL.LoadOrtho();
		
		mat.SetPass(pass);

        GL.Begin(GL.QUADS);
        GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(0.0f, 0.0f, 0.1f);
        GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(1.0f, 0.0f, 0.1f);
        GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(1.0f, 1.0f, 0.1f);
        GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(0.0f, 1.0f, 0.1f);
        GL.End();

        GL.PopMatrix();

        RenderTexture.active = oldRT;
    }

	/// <summary>
	/// Renders into a array of render textures using multi-target blit.
	/// Up to 4 render targets are supported in Unity but some GPU's can
	/// support up to 8 so this may change in the future. You MUST set up 
	/// the materials shader correctly for multitarget blit for this to work.
	/// </summary>
	/// <param name="des">The destination render textures.</param>
	/// <param name="mat">The amterial to use</param>
	/// <param name="pass">Which pass of the materials shader to use.</param>
    static public void MultiTargetBlit(RenderTexture[] des, Material mat, int pass = 0)
    {
 
		RenderBuffer[] rb = new RenderBuffer[des.Length];

		// Set targets needs the color buffers so make a array from 
		// each textures buffer.
		for(int i = 0; i < des.Length; i++)
			rb[i] = des[i].colorBuffer;

		//Set the targets to render into. 
		//Will use the depth buffer of the 
		//first render texture provided.
        Graphics.SetRenderTarget(rb, des[0].depthBuffer);

        GL.Clear(true, true, Color.clear);

        GL.PushMatrix();
        GL.LoadOrtho();
		
		mat.SetPass(pass);

        GL.Begin(GL.QUADS);
        GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(0.0f, 0.0f, 0.1f);
        GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(1.0f, 0.0f, 0.1f);
        GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(1.0f, 1.0f, 0.1f);
        GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(0.0f, 1.0f, 0.1f);
        GL.End();

        GL.PopMatrix();

    }

	/// <summary>
	/// Renders into a array of render buffers using multi-target blit
	/// and the provided depth buffer for depth testing.
	/// Up to 4 render targets are supported in Unity but some GPU's can
	/// support up to 8 so this may change in the future. You MUST set up 
	/// the materials shader correctly for multitarget blit for this to work.
	/// </summary>
	/// <param name="des_rb">The destination render buffers to use.</param>
	/// <param name="des_db">The destination depth buffer to use.</param>
	/// <param name="mat">The material to use</param>
	/// <param name="pass">Which pass of the materials shader to use.</param>
	static public void MultiTargetBlit(RenderBuffer[] des_rb, RenderBuffer des_db, Material mat, int pass = 0)
    {

		//Set the targets to render into. 
		//Will use the depth buffer provided, des_db.
        Graphics.SetRenderTarget(des_rb, des_db);

        GL.Clear(true, true, Color.clear);

        GL.PushMatrix();
        GL.LoadOrtho();
		
		mat.SetPass(pass);

        GL.Begin(GL.QUADS);
        GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(0.0f, 0.0f, 0.1f);
        GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(1.0f, 0.0f, 0.1f);
        GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(1.0f, 1.0f, 0.1f);
        GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(0.0f, 1.0f, 0.1f);
        GL.End();

        GL.PopMatrix();

    }


}



