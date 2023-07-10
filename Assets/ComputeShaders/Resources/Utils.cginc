struct VerticalArea
{
    int UpperLimit;
    int LowerLimit;
};

bool ColorsEquals(float4 c1, float4 c2)
{
    float epsilon = 0.01;
    return abs(c1.r - c2.r) < epsilon && abs(c1.g - c2.g) < epsilon && abs(c1.b - c2.b) < epsilon && abs(c1.a - c2.a) < epsilon;
}

bool Equals(float f1, float f2)
{
    float epsilon = 0.001;
    return abs(f1 - f2) < epsilon;
}