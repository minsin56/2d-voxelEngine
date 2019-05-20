using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

// updated to work with xml
public class VoxelType 
{
    [XmlAttribute("name")]
    public string name;
    [XmlAttribute("texid")]
    public int texid;
    [XmlAttribute("solid")]
    public bool solid;
    [XmlAttribute("liquid")]
    public bool liquid;
    [XmlAttribute("id")]
    public int id;
    [XmlAttribute]
    public bool collidable = true;
    [XmlAttribute]
    public bool placeoncvoxel = false;
}
