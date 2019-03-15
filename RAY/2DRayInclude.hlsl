struct Player
{
	float2 pos;
};
RWStructuredBuffer<Player> _Player;

float AO(float radius, float st, float d)
{
	float ao = clamp(d / radius, 0.0, 1.0) - 1.0;
	return 1.0 - (pow(abs(ao), 5.0) + 1.0) * st + (1.0 - radius);
}

float lineDist(float2 p, float2 start, float2 end, float width)
{
	float2 dir = start - end;
	float lngth = length(dir);
	dir /= lngth;
	float2 proj = max(0.0, min(lngth, dot((start - p), dir))) * dir;
	return length((start - p) - proj) - (width / 2.0);
}