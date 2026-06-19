using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace XiaoTiCompTicker
{
    public class MapComponent_AccelerationManager : MapComponent
    {
        private List<CompAccelerator> accelerators = new List<CompAccelerator>();
        private Dictionary<Thing, float> factorCache = new Dictionary<Thing, float>();
        private Dictionary<Thing, float> pawnFactorCache = new Dictionary<Thing, float>();
        private HashSet<Thing> _radiusThings = new HashSet<Thing>();
        private int lastUpdateTick = -9999;
        private const int UpdateIntervalTicks = 60;

        public int AcceleratorCount => accelerators.Count;

        public MapComponent_AccelerationManager(Map map) : base(map)
        {
            Log.Message($"[XiaoTiCompTicker] MapComponent_AccelerationManager created for map {map.uniqueID}");
        }

        public void Register(CompAccelerator acc)
        {
            if (!accelerators.Contains(acc))
            {
                accelerators.Add(acc);
                ForceRefresh();
            }
        }

        public void Unregister(CompAccelerator acc)
        {
            accelerators.Remove(acc);
            ForceRefresh();
        }

        public void ForceRefresh() => lastUpdateTick = -9999;

        // Total factor (all accelerators), for non-Pawn things
        public float GetFactor(Thing thing)
        {
            if (thing == null || thing.Destroyed || accelerators.Count == 0)
                return 1f;
            EnsureCacheValid();
            return factorCache.TryGetValue(thing, out float f) ? Mathf.Min(f, 16f) : 1f;
        }

        // Factor from accelerators with Pawn Boost enabled only, for Pawns
        public float GetPawnFactor(Thing thing)
        {
            if (thing == null || thing.Destroyed || accelerators.Count == 0)
                return 1f;
            EnsureCacheValid();
            return pawnFactorCache.TryGetValue(thing, out float f) ? Mathf.Min(f, 16f) : 1f;
        }

        private void EnsureCacheValid()
        {
            int currentTick = Find.TickManager.TicksGame;
            if (currentTick - lastUpdateTick < UpdateIntervalTicks)
                return;

            factorCache.Clear();
            pawnFactorCache.Clear();

            for (int ai = accelerators.Count - 1; ai >= 0; ai--)
            {
                var acc = accelerators[ai];
                if (acc == null || acc.parent == null || !acc.parent.Spawned || acc.parent.Destroyed)
                {
                    accelerators.RemoveAt(ai);
                    continue;
                }

                var parent = acc.parent;
                var map = parent.Map;
                if (map == null)
                    continue;

                float range = acc.Props.range;
                float factor = acc.Factor;
                float effRadius = Mathf.Min(range, GenRadial.MaxRadialPatternRadius - 0.01f);
                int numCells = GenRadial.NumCellsInRadius(effRadius);

                _radiusThings.Clear();
                for (int i = 0; i < numCells; i++)
                {
                    IntVec3 cell = GenRadial.RadialPattern[i] + parent.Position;
                    if (!cell.InBounds(map))
                        continue;
                    List<Thing> things = map.thingGrid.ThingsListAt(cell);
                    for (int j = 0; j < things.Count; j++)
                    {
                        Thing t = things[j];
                        if (t != null && !t.Destroyed)
                            _radiusThings.Add(t);
                    }
                }

                foreach (Thing t in _radiusThings)
                {
                    if (t == null || t.Destroyed)
                        continue;
                    factorCache.TryGetValue(t, out float existing);
                    factorCache[t] = Mathf.Min(existing + factor, 16f);

                    if (acc.enablePawnBoost)
                    {
                        pawnFactorCache.TryGetValue(t, out float pExisting);
                        pawnFactorCache[t] = Mathf.Min(pExisting + factor, 16f);
                    }
                }
            }

            lastUpdateTick = currentTick;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
                ForceRefresh();
        }
    }
}
