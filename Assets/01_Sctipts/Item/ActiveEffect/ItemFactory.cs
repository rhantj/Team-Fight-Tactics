using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemFactory
{
    public static ItemBase Create(ItemData data)
    {
        if(data.itemType == ItemType.Combined)
        {
            if(data.itemName == "Fimbulwinter")
            {
                return new FimbulWinter(data);
            }
            if (data.itemName == "SteraksGage")
            {
                return new SteraksGage(data);
            }
            if (data.itemName == "GuardianAngel")
            {
                return new GuardianAngel(data);
            }
            if (data.itemName == "NashorsTooth")
            {
                return new NashorsTooth(data);
            }
            if (data.itemName == "SpearOfShojin")
            {
                return new SpearOfShojin(data);
            }
            if (data.itemName == "GiantSlayer")
            {
                return new GiantSlayer(data);
            }
            if (data.itemName == "TitansResolve")
            {
                return new TitansResolve(data);
            }
            if (data.itemName == "VoidStaff")
            {
                return new VoidStaff(data);
            }
            if (data.itemName == "RedBuffItem")
            {
                return new RedBuff(data);
            }
            if (data.itemName == "BrambleVest")
            {
                return new BrambleVest(data);
            }
            if (data.itemName == "WarmogsArmor")
            {
                return new WarmogsArmor(data);
            }
            if(data.itemName == "SunfireCape")
            {
                return new SunFireCape(data);
            }
            if (data.itemName == "BlueBuffItem")
            {
                return new BlueBuff(data);
            }
        }

        return new ItemBase(data);
    }
}
