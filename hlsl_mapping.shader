float3 CylindricalMap(int textureID, float3 p)
{
    float2 uvScale = textureUVranges[textureID];
    float2 uv = float2(atan2(p.z, p.x) / 6.2832f, (p.y - 1.0f) * 0.5f);
    
    uv = glsl_mod(uv * uvScale, uvScale);

    return textures.SampleLevel(MyLinearRepeatSampler, float3(uv.x, uv.y, float(textureID)), 0);
}

float3 TriplanarMap(int textureID, float3 p, float k, float3 objectPosition)
{
    float3 obj_p = p - objectPosition;

    float2 uvScale = textureUVranges[textureID];

    float2 uvX = float2(obj_p.y, obj_p.z);
    uvX = glsl_mod(uvX * uvScale, uvScale);
    float3 x = textures.SampleLevel(MyLinearRepeatSampler, float3(uvX.x, uvX.y, (float)textureID), 0);
    
    float2 uvY = float2(obj_p.z, obj_p.x);
    uvY = glsl_mod(uvY * uvScale, uvScale);
    float3 y = textures.SampleLevel(MyLinearRepeatSampler, float3(uvY.x, uvY.y, (float)textureID), 0);

    float2 uvZ = float2(obj_p.x, obj_p.y);
    uvZ = glsl_mod(uvZ * uvScale, uvScale);
    float3 z = textures.SampleLevel(MyLinearRepeatSampler, float3(uvZ.x, uvZ.y, (float)textureID), 0);

    float3 n =abs(CalculateNormal(p));

    float3 w = pow(n, float3(k,k,k));

    return (x*w.x + y* w.y + z * w.z) / (w.x + w.y + w.z);
}

float3 BiplanarMap(int textureID, float3 p, float k, float3 objectPosition)
{
    float3 obj_p = p - objectPosition;

    float3 n = abs(CalculateNormal(p));
    float2 uvScale = textureUVranges[textureID];

    int3 ma = (n.x > n.y && n.x > n.z) ? int3(0, 1, 2) :
              (n.y > n.z)              ? int3(1, 2, 0) :
                                         int3(2, 0, 1) ;   

    int3 mi = (n.x < n.y && n.x < n.z) ? int3(0, 1, 2) :
              (n.y < n.z)              ? int3(1, 2, 0) :
                                         int3(2, 0, 1) ;

    int3 me = int3(3,3,3) - mi - ma;

    float2 uvX = float2(obj_p[ma.y], obj_p[ma.z]);
    uvX = glsl_mod(uvX * uvScale, uvScale);
    float3 x = textures.SampleLevel(MyLinearRepeatSampler, float3(uvX.x, uvX.y, (float)textureID), 0);

    float2 uvY = float2(obj_p[me.y], obj_p[me.z]);
    uvY = glsl_mod(uvY * uvScale, uvScale);
    float3 y = textures.SampleLevel(MyLinearRepeatSampler, float3(uvY.x, uvY.y, (float)textureID), 0);

    float2 w = float2(n[ma.x], n[me.x]);
    w = clamp((w - 0.5773)/ (1.0 - 0.5773), 0.0, 1.0);
    w = pow(w, float2(k,k));

    return (x*w.x + y * w.y) / (w.x + w.y);
}