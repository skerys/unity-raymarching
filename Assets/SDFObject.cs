using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDFObject : MonoBehaviour
{
    public enum SDFType {Sphere, Box, Torus, Mandelbulb, Juliabulb};
    public enum CombineOperation {Add, SmoothAdd, Cut, Mask};
    public enum ModifierType {None, Elongate, Round, Onion, Repetition, Displacement, Twist}
    public enum TextureMappingType{Cylindrical, Triplanar, Biplanar}

    public SDFType shapeType;
    public CombineOperation combineOperation;
    public ModifierType modifier;
    public Vector3 modifierVar;
    public Color color = Color.white;

    [Range(0,1)]
    public float blendFactor;
    public float smoothness;

    public TextureMappingType textureMappingType;
    public Texture2D texture;
    public int textureID = -1;


    [HideInInspector]
    public int childrenCount;

    public Vector3 Position{
        get{
            return transform.position;
        }
    }

    public Vector3 Scale{
        get{
            Vector3 parentScale = Vector3.one;
            if(transform.parent != null && transform.parent.GetComponent<SDFObject>() != null){
                parentScale = transform.parent.GetComponent<SDFObject>().Scale;
            }
            return Vector3.Scale(transform.localScale, parentScale);
        }
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        switch(shapeType){
            case SDFType.Sphere:
                Gizmos.DrawWireSphere(transform.position, transform.lossyScale.x);
                break;
            case SDFType.Box:
                Gizmos.DrawWireCube(transform.position, transform.lossyScale * 2.0f);
                break;
            case SDFType.Torus:
                Gizmos.DrawWireSphere(transform.position, transform.lossyScale.x + transform.lossyScale.y);
                break;
            default:
                break;
        }
    }
}
