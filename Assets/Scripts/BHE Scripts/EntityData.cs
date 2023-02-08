using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityData : ScriptableObject
{
    public const string EntityType = "DEFAULT_ENTITY_TYPE";
    public virtual string GetEntityType() => EntityType;


    [Header("Base Vars")]
    [Header("Animation Vars")]
    public List<Texture2D> Sprites;

    public int Frames = 1;
    public float Fps = 1;

    /*
    TODO: support sound objects 
    via List<>
    */

    [Header("Collision Vars")]
    Collider2D Collider;
}
