#define PI 3.14159265359
#define TAU (2*PI)
#define PHI (1.618033988749895)
#define INV_PHI 0.6180339887

#define vec2 float2
#define vec3 float3
#define fract frac
#define mix lerp
//------------------------------------------------------------------ [Box]
float sdBox(float3 p, float3 b)
{
	float3 d = abs(p) - b;
	return min(max(d.x, max(d.y, d.z)), 0) + length(max(d, 0));
}

float sdBox2D(float2 p, float2 b)
{
	float2 d = abs(p) - b;
	return min(max(d.x, d.y), 0) + length(max(d, 0));
}

float sdBox1D(float p, float b)
{
	float d = abs(p) - b;
	return min(d, 0) + max(d, 0);
}

float udBox(float3 p, float3 b)
{
	return length(max(abs(p) - b, 0.0));
}

float udBox2D(float2 p, float2 b)
{
	return length(max(abs(p) - b, 0.0));
}

float udBox1D(float p, float b)
{
	return max(abs(p) - b, 0.0);
}
//------------------------------------------------------------------ [Cross]
float sdCross(float3 p, float3 b)
{
	float d1 = sdBox(p, b);
	float d2 = sdBox(p, b);
	float d3 = sdBox(p, b);

	return min(min(d1, d2), d3);
}

float sdCross2D(float3 p, float3 b)
{
	float d1 = sdBox2D(p.yz, b.yz);
	float d2 = sdBox2D(p.zx, b.zx);
	float d3 = sdBox2D(p.xy, b.xy);

	return min(min(d1, d2), d3);
}

float sdCross1D(float3 p, float3 b)
{
	float d1 = sdBox1D(p.x, b.x);
	float d2 = sdBox1D(p.y, b.y);
	float d3 = sdBox1D(p.z, b.z);

	return min(min(d1, d2), d3);
}

float udCross(float3 p, float3 b)
{
	float d1 = udBox(p, b);
	float d2 = udBox(p, b);
	float d3 = udBox(p, b);

	return min(min(d1, d2), d3);
}

float udCross2D(float3 p, float3 b)
{
	float d1 = udBox2D(p.yz, b.yz);
	float d2 = udBox2D(p.zx, b.zx);
	float d3 = udBox2D(p.xy, b.xy);

	return min(min(d1, d2), d3);
}

float udCross1D(float3 p, float3 b)
{
	float d1 = udBox1D(p.x, b.x);
	float d2 = udBox1D(p.y, b.y);
	float d3 = udBox1D(p.z, b.z);

	return min(min(d1, d2), d3);
}
//------------------------------------------------------------------ [Sphere]
float sdSphere(float3 p, float s)
{
	return length(p) - s;
}

float sdSphere2D(float2 p, float s)
{
	return length(p) - s;
}
//------------------------------------------------------------------ [Operations]
float mod(float x, float y)
{
	return(x - y * floor(x / y));
}

float2 mod(float2 x, float2 y)
{
	return(x - y * floor(x / y));
}

float3 mod(float3 x, float3 y)
{
	return(x - y * floor(x / y));
}

float opRep(float p, float size) 
{
	return mod(p.x, size * 2) - size;
}

float sdPlane(float3 p, float4 n)
{
	return dot(p, n.xyz) + n.w;
}

float sdTorus(float3 p, float2 t)
{
	float2 q = float2(length(p.xz) - t.x, p.y);
	return length(q) - t.y;
}

float2 opRepPolar(float2 p, float repetitions) {
	float angle = 2 * PI / repetitions;
	float a = atan2(p.y, p.x) + angle * .5;
	float r = length(p);
	//float c = floor(a / angle);
	a = mod(a, angle) - angle * .5;
	p = float2(cos(a), sin(a)) * r;
	//if (abs(c) >= (repetitions / 2)) c = abs(c);
	//return c;
	return p;
}

float3 opTwist(float3 p, float a)
{
	float c = cos(a * p.y);
	float s = sin(a * p.y);
	float2x2  m = float2x2(c, -s, s, c);
	p = float3(mul(m, p.xz), p.y);

	return p;
}
//------------------------------------------------------------------
float rand(float p)
{
	return frac(sin(p) * 43758.5453);
}

float2 rand(float2 p)
{
	p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
	return frac(sin(p) * 43758.5453);
}

float3 rand(float3 p)
{
	p = float3(dot(p, float3(127.1, 311.7, 475.6)), dot(p, float3(269.5, 676.5, 475.6)), dot(p, float3(318.5, 183.3, 713.4)));
	return frac(sin(p) * 43758.5453);
}

float opBend(float3 p, float3 b, float2 a)
{
	float c = cos(a.x * p.y);
	float s = sin(a.y * p.y);
	float2x2  m = float2x2(c, -s, s, c);
	float3  q = float3(mul(m, p.xy), p.z);
	return sdBox(q, b);
}

float3 rain(float a, float t)
{
	return sin((frac((a / PI) * .5 + .5) + t + .75/**/ +
		float3(.0, .33333333, .66666666)) * PI * 2.) * .5 + .5;
}


/*void rX(inout vec3 p, float a) {
	float c, s; vec3 q = p;
	c = cos(a); s = sin(a);
	p.y = c * q.y - s * q.z;
	p.z = s * q.y + c * q.z;
}

void rY(inout vec3 p, float a) {
	float c, s; vec3 q = p;
	c = cos(a); s = sin(a);
	p.x = c * q.x + s * q.z;
	p.z = -s * q.x + c * q.z;
}

void rZ(inout vec3 p, float a) {
	float c, s; vec3 q = p;
	c = cos(a); s = sin(a);
	p.x = c * q.x - s * q.y;
	p.y = s * q.x + c * q.y;
}
void rXCS(inout vec3 p, float c, float s) {
	vec3 q = p;
	p.y = c * q.y - s * q.z;
	p.z = s * q.y + c * q.z;
}


void rYCS(inout vec3 p, float c, float s) {
	vec3 q = p;
	p.x = c * q.x + s * q.z;
	p.z = -s * q.x + c * q.z;
}

void rZCS(inout vec3 p, float c, float s) {
	vec3 q = p;
	p.x = c * q.x - s * q.y;
	p.y = s * q.x + c * q.y;
}*/