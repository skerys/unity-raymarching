using System.Collections.Generic;
using UnityEngine;

struct SDFObjectDataVolumeProto
{
    public Vector3 position;
    public Vector3 scale;
    public Vector3 color;
    public int objType;
    public int combineOp;
    public float blendFactor;
    public float smoothness;
    public int childrenCount;
    public int modifierType;
    public Vector3 modifierVar1;

    public static int GetSize(){
        return sizeof(float) * 14 + sizeof(int) * 4;
    }
}

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class RaymarchSetupVolumeProto : MonoBehaviour
{
    public ComputeShader raymarcherShader;
    public Color ambientColor = Color.black;

    Camera cam;
    RenderTexture targetRT;
    Light lightObj;
    List<ComputeBuffer> buffers;

    public Texture3D volumeTexture;
    

    void Initialize()
    {
        cam = Camera.current;
        lightObj = FindObjectOfType<Light>();
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Initialize();
        buffers = new List<ComputeBuffer>();

        //Create the Render Texture if there is none or resolution changed
        if(targetRT == null || targetRT.width != cam.pixelWidth || targetRT.height != cam.pixelHeight)
        {
            if(targetRT != null)
            {
                targetRT.Release();
            }
            targetRT = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            targetRT.enableRandomWrite = true;
            targetRT.Create();
        }

        if(volumeTexture) raymarcherShader.SetTexture(0, "volumeTexture", volumeTexture);
        SetupSceneObjects();
        SetupLights();

        raymarcherShader.SetTexture(0, "Source", source);
        raymarcherShader.SetTexture(0, "Result", targetRT);

        int threadGroupsX = Mathf.CeilToInt(cam.pixelWidth / 16.0f);
        int threadGroupsY = Mathf.CeilToInt(cam.pixelHeight / 16.0f);
        raymarcherShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        Graphics.Blit(targetRT, destination);

        foreach(var buffer in buffers)
        {
            buffer.Dispose();
        }
    }
 
    void SetupSceneObjects()
    {
        List<SDFObjectVolumeProto> sceneObjects = new List<SDFObjectVolumeProto>(FindObjectsOfType<SDFObjectVolumeProto>());
        sceneObjects.Sort((a,b) => a.combineOperation.CompareTo(b.combineOperation));

        List<SDFObjectVolumeProto> orderedObjects = new List<SDFObjectVolumeProto>();

        for(int i = 0; i < sceneObjects.Count; ++i)
        {
            if(sceneObjects[i].transform.parent != null) continue;

            Transform parentObj = sceneObjects[i].transform;
            orderedObjects.Add(sceneObjects[i]);

            sceneObjects[i].childrenCount = parentObj.childCount;
            for(int j = 0; j < parentObj.childCount; j++){
                var childObject = parentObj.GetChild(j).GetComponent<SDFObjectVolumeProto>();
                if(childObject!= null){
                    orderedObjects.Add(childObject);
                    orderedObjects[orderedObjects.Count - 1].childrenCount = 0;
                }
            }
        }

        SDFObjectDataVolumeProto[] objectData = new SDFObjectDataVolumeProto[orderedObjects.Count];
        for(int i = 0; i < orderedObjects.Count; ++i)
        {
            var obj = orderedObjects[i];
            Vector3 col = new Vector3(obj.color.r, obj.color.g, obj.color.b);
            objectData[i] = new SDFObjectDataVolumeProto(){
                position = obj.Position,
                scale = obj.Scale,
                color = col,
                objType = (int) obj.shapeType,
                combineOp = (int) obj.combineOperation,
                blendFactor = obj.blendFactor,
                childrenCount = (int)obj.childrenCount,
                smoothness = obj.smoothness,
                modifierType = (int)obj.modifier,
                modifierVar1 = obj.modifierVar,
            };
        }

        ComputeBuffer sdfBuffer = new ComputeBuffer(objectData.Length, SDFObjectDataVolumeProto.GetSize());
        sdfBuffer.SetData(objectData);
        raymarcherShader.SetBuffer(0, "objects", sdfBuffer);
        raymarcherShader.SetInt("numObjects", objectData.Length);

        buffers.Add(sdfBuffer);

        raymarcherShader.SetMatrix("CameraToWorldMatrix", cam.cameraToWorldMatrix);
        raymarcherShader.SetMatrix("CameraInverseProjectionMatrix", cam.projectionMatrix.inverse);
        raymarcherShader.SetVector("AmbientColor", ambientColor);
    }

    void SetupLights()
    {
        List<Light> lights = new List<Light>(FindObjectsOfType<Light>());

        LightData[] lightData = new LightData[lights.Count];
        for(int i = 0; i <lights.Count; ++i)
        {
            var light = lights[i];
            Vector3 intensity = new Vector3(light.color.r, light.color.g, light.color.b) * light.intensity;
            lightData[i] = new LightData(){
                position = light.type == LightType.Directional ? light.transform.forward : light.transform.position,
                intensity = intensity,
                isDirectional = light.type == LightType.Directional ? 1:0
            };
        }
        ComputeBuffer lightBuffer = new ComputeBuffer(lightData.Length, LightData.GetSize());
        lightBuffer.SetData(lightData);
        raymarcherShader.SetBuffer(0, "lights", lightBuffer);
        raymarcherShader.SetInt("numLights", lightData.Length);

        buffers.Add(lightBuffer);
    }
}
