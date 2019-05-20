using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
public struct Explosion
{
    [XmlAttribute]
    string name;
    [XmlAttribute]
    int radius;
}
