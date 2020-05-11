Shader "Hidden/DPLayout/Multiply"
{

	Properties
	{
		_Color("Color", Vector) = (1,1,1,1)
	}

		SubShader
	{
		Tags{ Queue = Transparent }
		Blend Zero SrcColor
		Pass{ Color[_Color] }
	}

}