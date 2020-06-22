
#ifdef SHADERPASS // detect the shadow pass in HDRP and URP
	#if SHADERPASS == SHADERPASS_SHADOWS
		#define SHADERPASS_SHADOWCASTER
	#endif
#endif

#ifdef UNITY_PASS_SHADOWCASTER // detect shadow pass in built-in
	#define SHADERPASS_SHADOWCASTER
#endif

#if defined (SHADER_API_D3D11) || defined(SHADER_API_METAL)
	#define CLAYXELS_VALID
#endif

#ifdef CLAYXELS_VALID
	uniform StructuredBuffer<int4> chunkPoints;
#endif 

float4x4 objectMatrix;
float3 chunkCenter;
float chunkSize = 0.0;
float splatRadius = 0.01;
int solidHighlightId;

int bytes4ToInt(uint a, uint b, uint c, uint d){
	int retVal = (a << 24) | (b << 16) | (c << 8) | d;
	return retVal;
}

int4 unpackInt4(uint inVal){
	uint r = inVal >> 24;
	uint g = (0x00FF0000 & inVal) >> 16;
	uint b = (0x0000FF00 & inVal) >> 8;
	uint a = (0x000000FF & inVal);

	return int4(r, g, b, a);
}

float3 unpackFloat3(float f){
	return frac(f / float3(16777216, 65536, 256));
}

float3 expandGridPoint(int3 cellCoord, float cellSize, float localChunkSize){
	float cellCornerOffset = cellSize * 0.5;
	float halfBounds = localChunkSize * 0.5;
	float3 gridPoint = float3(
		(cellSize * cellCoord.x) - halfBounds, 
		(cellSize * cellCoord.y) - halfBounds, 
		(cellSize * cellCoord.z) - halfBounds) + cellCornerOffset;

	return gridPoint;
}

float2 unpackFloat2(float input){
	int precision = 2048;
	float2 output = float2(0.0, 0.0);

	output.y = input % precision;
	output.x = floor(input / precision);

	return output / (precision - 1);
}

float3 unpackNormal(float fSingle){
	float2 f = unpackFloat2(fSingle);

	f = f * 2.0 - 1.0;

	float3 n = float3( f.x, f.y, 1.0 - abs( f.x ) - abs( f.y ) );
	float t = saturate( -n.z );
	n.xy += n.xy >= 0.0 ? -t : t;

	return normalize( n );
}

float3 unpackRgb(uint inVal){
	int r = (inVal & 0x000000FF) >>  0;
	int g = (inVal & 0x0000FF00) >>  8;
	int b = (inVal & 0x00FF0000) >> 16;

	return float3(r/255.0, g/255.0, b/255.0);
}

void clayxelGetPointCloud(uint vId, out float3 gridPoint, out float3 pointColor, out float3 pointCenter, out float3 pointNormal){
#ifdef CLAYXELS_VALID
	int4 clayxelPointData = chunkPoints[vId / 3];
	
	pointNormal = mul((float3x3)objectMatrix, unpackNormal(clayxelPointData.z));

	int4 compressedData = unpackInt4(clayxelPointData.x);

	float cellSize = chunkSize / 256.0;
	float3 cellLocalOffset = unpackFloat3(clayxelPointData.y) * cellSize;

	gridPoint = compressedData.xyz;
	
	float3 pointPos = expandGridPoint(compressedData.xyz, cellSize, chunkSize) + cellLocalOffset + chunkCenter;
	pointCenter = mul(objectMatrix, float4(pointPos, 1.0)).xyz;

	pointColor = unpackRgb(clayxelPointData.w);
#else
gridPoint = float3(0, 0, 0);
pointColor = float3(0, 0, 0);
pointCenter = float3(0, 0, 0);
pointNormal = float3(0, 0, 0);
#endif
}

void clayxelVertNormalBlend(uint vId, float splatSizeMult, float normalOrientedSplat, out float4 tex, out float3 vertexColor, out float3 outVertPos, out float3 outNormal){
#ifdef CLAYXELS_VALID
	// first we unpack the clayxels point cloud
	int4 clayxelPointData = chunkPoints[vId / 3];

	outNormal = mul((float3x3)objectMatrix, unpackNormal(clayxelPointData.z));

	int4 compressedData = unpackInt4(clayxelPointData.x);

	float cellSize = chunkSize / 256.0;
	float3 cellLocalOffset = unpackFloat3(clayxelPointData.y) * cellSize;
	float3 pointPos = expandGridPoint(compressedData.xyz, cellSize, chunkSize) + cellLocalOffset + chunkCenter;
	float3 p = mul(objectMatrix, float4(pointPos, 1.0)).xyz;

	vertexColor = unpackRgb(clayxelPointData.w);

	int solidId = compressedData.w - 1;
	if(solidId == solidHighlightId){
		vertexColor += 1.0;
	}

	float newSplatSize = splatRadius * splatSizeMult;
	float3 camUpVec = normalize(UNITY_MATRIX_V._m10_m11_m12);
	float3 camSideVec =  normalize(UNITY_MATRIX_V._m00_m01_m02);
	
	float3 upVec;
	float3 sideVec;

	#if defined(SHADERPASS_SHADOWCASTER) // on shadowPass force splats orientating to normals to prevent holes in the shadows
		sideVec = normalize(cross(camUpVec, outNormal)) * (newSplatSize*2.0);
		upVec = normalize(cross(sideVec, outNormal)) * newSplatSize;
	#else
		if(normalOrientedSplat == 0.0){// billboard splats
			float3 eyeVec = normalize(_WorldSpaceCameraPos - p);
			camUpVec = normalize(cross(camSideVec, eyeVec));
			camSideVec = normalize(cross(eyeVec, camUpVec));

			upVec = camUpVec * (newSplatSize);
			sideVec = camSideVec * (newSplatSize * 2.0);
		}
		else{// normal oriented splats
			float3 normalSideVec = normalize(cross(camUpVec, outNormal));
			float3 normalUpVec = normalize(cross(normalSideVec, outNormal));
			
			if(normalOrientedSplat == 1.0){// fully normal oriented
				upVec = normalUpVec * (newSplatSize);
				sideVec = normalSideVec * (newSplatSize * 2.0);
			}
			else{// interpolated normal orient
				upVec = normalize(lerp(camUpVec, normalUpVec, normalOrientedSplat)) * (newSplatSize);
				sideVec = normalize(lerp(camSideVec, normalSideVec, normalOrientedSplat)) * (newSplatSize*2.0);
			}
		}
	#endif

	// expand splat from point P to a triangle with uv coordinates
	uint vertexOffset = vId % 3;
	if(vertexOffset == 0){
		outVertPos = p + ((-upVec) + sideVec);
		tex = float4(-0.5, 0.0, 0.0, 0.0);
	}
	else if(vertexOffset == 1){
		outVertPos = p + ((-upVec) - sideVec);
		tex = float4(1.5, 0.0, 0.0, 0.0);
	}
	else if(vertexOffset == 2){
		outVertPos = p + (upVec * 1.7);
		tex = float4(0.5, 1.35, 0.0, 0.0);
	}

	#if !defined(SHADERPASS_SHADOWCASTER) 
		if(normalOrientedSplat > 0.0){// flatten splats against eye-depth to avoid ugly intersections
			float3 eyeVec = normalize(_WorldSpaceCameraPos - p);
			outVertPos = outVertPos - (eyeVec * (dot(eyeVec, outVertPos - p)));
		}
	#endif
#else
		tex = float4(0, 0, 0, 0);
		vertexColor = float3(0, 0, 0);
		outVertPos = float3(0, 0, 0);
		outNormal = float3(0, 0, 0);
#endif
}

float random(float2 uv){
    return frac(sin(dot(uv,float2(12.9898,78.233)))*43758.5453123);
}

void srpSplatTexture(float2 vertexUV, float textureAlpha, out float outAlpha){
	outAlpha = textureAlpha;

	if(textureAlpha > 0.0){
		if(random(vertexUV) > textureAlpha){
			outAlpha = 0.0;
		}
		else{
			outAlpha = 1.0;
		}
	}
}

void clayxelFrag(float3 vertexColor, float4 vertexUV, float textureAlpha, out float outAlpha){
	outAlpha = 1.0;

	#if SPLATTEXTURE_ON // if textured splats, randomly discard pixels based on alpha amount
		srpSplatTexture(vertexUV.xy, textureAlpha, outAlpha);
	#else// default is round point splat, just discard around radius
		if(length(vertexUV.xy -0.5) > 0.5){
			outAlpha = 0.0;
		}
	#endif
}
