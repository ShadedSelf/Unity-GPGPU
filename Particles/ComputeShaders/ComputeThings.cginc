#define PI 3.1415926535f
static const float3 ZERO = float3(0, 0, 0);

struct Particle
{
	float4 pos;
	float4 p;
};

struct CollisionData
{
	uint cell;
	uint particleID;
};

struct Grid
{
	uint start;
	uint end;
};

struct Cubes
{
	float4 position;
	float4 scale;
	float4x4 rot;
};

struct Nei
{
	uint num;
	uint neis[64];
};


	//float3 bw = (float3)collisionBuffer[id.x].cell / (gridSize * gridSize * gridSize - 1);
	//float3 col = (float3)1 - (float3)gridTemp / (gridSize - 1);
	//float3 wge = (float3)id.x / (float3)particleCount;


float normalizeFloat(float value, float min, float max)
{
	return (value - min) / (max - min);
}

float3 normalizeFloat3(float3 value, float3 min, float3 max)
{
	return (value - min) / (max - min);
}

uint fromGridToIndex(uint3 grid, int3 gridSize)
{
	return grid.x + (grid.y * gridSize.x) + (grid.z * gridSize.x * gridSize.y);
	// const uint p1 = 73856093;
	// const uint p2 = 19349663;
	// const uint p3 = 83492791;
	// uint n = p1 * grid.x ^ p2*grid.y ^ p3*grid.z; //resolutino
	// n %= (gridSize.x * gridSize.x * gridSize.x);
	// return n;
}

uint3 fromIndexToGrid(uint binID, uint3 num) 
{
	// uint3 binID3D;
	// binID3D.z = binID / (num.x * num.y);
	// binID3D.y = (binID - binID3D.z * num.x * num.y) / num.x;
	// binID3D.x = binID - num.x * (binID3D.y + num.y * binID3D.z);
	// return binID3D;

	uint3 res;
	res.x = binID % num.x;
	res.y = (binID / num.x) % num.y;
	res.z = binID / (num.x * num.y);
	return res;
}

bool checkAABB(float3 aPos, float3 aScale, float3 bPos, float3 bScale)
{
	return ((aPos.x - aScale.x <= bPos.x + bScale.x && aPos.x + aScale.x >= bPos.x - bScale.x) && 
			(aPos.y - aScale.y <= bPos.y + bScale.y && aPos.y + aScale.y >= bPos.y - bScale.y) && 
			(aPos.z - aScale.z <= bPos.z + bScale.z && aPos.z + aScale.z >= bPos.z - bScale.z));
}

float4x4 rotationVectorToMatrix(float3 rot)
{
	float sir = sin(rot.z);
	float sor = cos(rot.z);
	float4x4 zRot;
	zRot._11_21_31_41 = float4(sor, -sir, 0, 0);
    zRot._12_22_32_42 = float4(sir, sor, 0, 0);
    zRot._13_23_33_43 = float4(0, 0, 1, 0);
	zRot._14_24_34_44 = float4(0, 0, 0, 1);

	sir = sin(rot.y);
	sor = cos(rot.y);
	float4x4 yRot;
	yRot._11_21_31_41 = float4(sor, 0, sir, 0);
    yRot._12_22_32_42 = float4(0, 1, 0, 0);
    yRot._13_23_33_43 = float4(-sir, 0, sor, 0);
    yRot._14_24_34_44 = float4(0, 0, 0, 1);

		
	sir = sin(rot.x);
	sor = cos(rot.x);
	float4x4 xRot;
	xRot._11_21_31_41 = float4(1, 0, 0, 0);
    xRot._12_22_32_42 = float4(0, sor, -sir, 0);
    xRot._13_23_33_43 = float4(0, sir, sor, 0);
    xRot._14_24_34_44 = float4(0, 0, 0, 1);

	return mul(mul(zRot, yRot), xRot);
}

float W(float3 r, float h)
{
	return (length(r) >= 0 && h >= length(r)) ?
		(315 / (64 * PI * pow(h, 9))) * pow((pow(h, 2) - pow(length(r), 2)), 3) :
		0;
}

float3 WS(float3 r, float h)
{
	return (length(r) > 0 && h >= length(r)) ?
		-(45 / (PI * pow(h, 6))) * pow((h - length(r)), 2) * (r / length(r)) :
		ZERO;
}

float sdBox(float3 p, float3 b)
{
	float3 d = abs(p) - b;
	return min(max(d.x, max(d.y, d.z)), 0) + length(max(d, 0));
}

float sdSphere(float3 p, float s)
{
	return length(p) - s;
}

float sdTorus(float3 p, float2 t)
{
	float2 q = float2(length(p.xz) - t.x, p.y);
	return length(q) - t.y;
}
