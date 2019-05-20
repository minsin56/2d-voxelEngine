using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

public struct Meteor
{
    [XmlAttribute]
    public string name;
    [XmlAttribute]
    public int size;
    [XmlAttribute]
    public float mass;
}
