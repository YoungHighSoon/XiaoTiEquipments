using Verse;

namespace XiaoTiEquipment
{
    public class Verb_ShootWearableTurret : Verb_Shoot
    {
        public override bool CasterIsPawn => false;
    }
}
