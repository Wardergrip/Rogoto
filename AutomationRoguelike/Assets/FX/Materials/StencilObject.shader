Shader "Stencil/Object"
{
	SubShader
	{
		// The rest of the code that defines the SubShader goes here.

	   Pass
	   {
			Stencil
			{
				Ref 1
				Comp Equal
			}
   }
}
}