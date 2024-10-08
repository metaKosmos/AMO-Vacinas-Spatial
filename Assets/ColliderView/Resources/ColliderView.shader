﻿Shader "ColliderView"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
	}

	SubShader
	{
		Pass 
		{ 
			ZWrite Off 
			Cull Off 
			Fog { Mode Off } 
			Blend SrcAlpha OneMinusSrcAlpha
			Colormask RGBA 
			Lighting Off 
			Color[_Color]
		}
	} 
}