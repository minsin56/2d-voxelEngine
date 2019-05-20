using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
[XmlRoot("VoxelDatabase")]
public class VoxelDatabase
{
    [XmlArray("voxels")]
    [XmlArrayItem("voxel")]
    public List<VoxelType> voxelTypes = new List<VoxelType>();

    // a VoxelDatabase reference is requierd now unil i can find a way so a reference isn't requierd or someone finds a way
    public static VoxelDatabase loadVoxelTypes()
    {
        TextAsset _xml = Resources.Load<TextAsset>("Data/Voxels");
        XmlSerializer serializer = new XmlSerializer(typeof(VoxelDatabase));
        StringReader reader = new StringReader(_xml.text);
        VoxelDatabase db = serializer.Deserialize(reader) as VoxelDatabase;
        reader.Close();

        return db;
    }
}