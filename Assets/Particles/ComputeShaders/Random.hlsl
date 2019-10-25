
uint iSeed;

float rand()
{
	iSeed += (iSeed << 10u);
	iSeed ^= (iSeed >>  6u);
	iSeed += (iSeed <<  3u);
	iSeed ^= (iSeed >> 11u);
	iSeed += (iSeed << 15u);

	const uint ieeeMantissa = 0x007FFFFFu;
	const uint ieeeOne      = 0x3F800000u;

	uint tmp = iSeed;
	tmp &= ieeeMantissa;
	tmp |= ieeeOne;

	return asfloat(tmp) - 1.0;
}

float  rand1() { return rand(); }
float2 rand2() { return float2(rand(), rand()); }
float3 rand3() { return float3(rand(), rand(), rand()); }
float3 rand4() { return float4(rand(), rand(), rand(), rand()); }