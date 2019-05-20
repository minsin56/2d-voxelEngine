using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

namespace voxelmine.inventory
{
    public enum itemType
    {
        [XmlEnum(Name = "misc")]
        misc,
        [XmlEnum(Name = "consumable")]
        consumable,
        [XmlEnum(Name = "tool")]
        tool,
        [XmlEnum(Name = "weapon")]
        weapon,
        [XmlEnum(Name = "rangedWeapon")]
        rangedWeapon,
        [XmlEnum(Name = "placeable")]
        placeable

    }
    public abstract class Item
    {
        [XmlAttribute]
        public int id;
        [XmlAttribute]
        itemType type;
        [XmlAttribute]
        public string name;
        [XmlAttribute]
        public string sprite;
        [XmlAttribute]
        public int max_stack;

        public virtual void onuse() { }
    }
    public class consumable : Item
    {
        [XmlElement]
        public float heathRegain;
        public override void onuse()
        {
            Player p = Player.findPlayer();
            if(p.health + heathRegain <-100)
            {
                p.health += heathRegain;
            }
            else if(p.health+heathRegain > 100 && p.health < 100)
            {
                p.health += p.health - heathRegain;
            }
        }
    }
    public class weapon : Item
    {
        [XmlElement]
        public float uses;
        [XmlElement]
        public float damage;

        private Camera cam;
        public override void onuse()
        {
            cam = Camera.main;
            Vector2 hpos =cam.ScreenToWorldPoint(Input.mousePosition, cam.stereoActiveEye);
            Debug.Log(hpos);
        }
    }

}