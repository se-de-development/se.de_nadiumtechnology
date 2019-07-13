using System;
using System.Collections.Generic;
using System.Text;
using VRage.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRageMath;
using VRage.Game.ModAPI;
using Sandbox.ModAPI;

namespace JumpDriveInhibitor
{
    //[MyEntityComponentDescriptor(typeof(Sandbox.Common.ObjectBuilders.MyObjectBuilder_Beacon))]
    [MyEntityComponentDescriptor(typeof(Sandbox.Common.ObjectBuilders.MyObjectBuilder_Beacon), true, new string[] { "Beacon", "JumpInhibitor" })]
    public class JumpDriveInhibitorBlock : MyGameLogicComponent
    {

        private VRage.ObjectBuilders.MyObjectBuilder_EntityBase _objectBuilder;
        private DateTime lastUpdate = DateTime.MinValue;
        private Sandbox.ModAPI.Ingame.IMyBeacon beacon;
        private IMyEntity entity;
        private System.IO.TextWriter logger = null;
        private String timeofload = "" + DateTime.Now.Year + "." + DateTime.Now.Month + "." + DateTime.Now.Day + " " + DateTime.Now.Hour + "." + DateTime.Now.Minute + "." + DateTime.Now.Second;
        private bool logicEnabled = false;
        public override void Close()
        {

        }

        public override void Init(VRage.ObjectBuilders.MyObjectBuilder_EntityBase objectBuilder)
        {
            _objectBuilder = objectBuilder;

            beacon = (Entity as Sandbox.ModAPI.Ingame.IMyBeacon);

            if (beacon != null && beacon.BlockDefinition.SubtypeId.Equals("JumpInhibitor"))
            {
                logicEnabled = true;
                Entity.NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;
            }
            
        }

        public override void MarkForClose()
        {
        }

        public override void UpdateAfterSimulation()
        {
        }

		public override void UpdateBeforeSimulation()        
        {
        }

        public override void UpdateAfterSimulation100()
        {
        }

        public override void UpdateAfterSimulation10()
        {
           try {

                //if (DateTime.Now - lastUpdate < TimeSpan.FromMilliseconds(250)) return;

                if (!logicEnabled || beacon == null || !beacon.Enabled || !beacon.IsWorking || !beacon.IsFunctional) return;
                
                List<IMyEntity> l = new List<IMyEntity>();

                BoundingSphereD sphere = new BoundingSphereD((Entity as Sandbox.ModAPI.Ingame.IMyBeacon).GetPosition(), (Entity as Sandbox.ModAPI.Ingame.IMyBeacon).Radius);
                l = MyAPIGateway.Entities.GetEntitiesInSphere(ref sphere);

				var parentGrid = beacon.CubeGrid;
                if(entity==null)
                    entity = MyAPIGateway.Entities.GetEntityById(parentGrid.EntityId);

                if (parentGrid != null && parentGrid.GridSizeEnum.Equals(MyCubeSize.Large)                    
                           && (parentGrid.IsStatic || (entity.Physics != null && entity.Physics.LinearVelocity != null && entity.Physics.LinearVelocity.Length() == 0)))
                {
                int i = 0;

                foreach (IMyEntity e in l)
                {
                    IMyCubeGrid grid = (e as IMyCubeGrid);
                        
                    if (grid != null)
                    {
                        List<IMySlimBlock> blocks = new List<IMySlimBlock>();
                        grid.GetBlocks(blocks, b => b != null && b.FatBlock != null && (b.FatBlock as Sandbox.ModAPI.Ingame.IMyJumpDrive) != null && b.FatBlock.IsWorking && b.FatBlock.IsFunctional &&
                            b.FatBlock.BlockDefinition.ToString().Contains("MyObjectBuilder_JumpDrive"));

                        foreach (IMySlimBlock b in blocks)
                        {
                            if ((b.FatBlock as Sandbox.ModAPI.Ingame.IMyJumpDrive).Enabled)
                            {
                                var damage = grid.GridSizeEnum.Equals(MyCubeSize.Large) ? 0.5f : 0.05f;
                                b.DecreaseMountLevel(damage, null, true);
                                b.ApplyAccumulatedDamage();
                                
                                   (b.FatBlock as Sandbox.ModAPI.Ingame.IMyJumpDrive).RequestEnable(false);
                            }
                            i++;
                        }
                    }
                }
				}
            }
            catch (Exception e)
            {
                MyAPIGateway.Utilities.ShowMessage("Jumpdrive Inhibitor", "An error happened in the mod");
            }
                //todo: export i to custom info text
                //lastUpdate = DateTime.Now;
           
        }

        public override VRage.ObjectBuilders.MyObjectBuilder_EntityBase GetObjectBuilder(bool copy = false)
        {
            return _objectBuilder;
        }
        private void log(string text)
        {
            if (logger == null)
            {
                try
                {
                    logger = MyAPIGateway.Utilities.WriteFileInLocalStorage(this.GetType().Name + "-" + timeofload + ".log", this.GetType());
                }
                catch (Exception)
                {
                    MyAPIGateway.Utilities.ShowMessage("AICombatLib", "Could not open the log file:" + this.GetType().Name + "-" + timeofload + ".log");
                    return;
                }
            }

            String datum = DateTime.Now.Year + "." + DateTime.Now.Month + "." + DateTime.Now.Day + " " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second;
            logger.WriteLine(datum + ": " + text);
            logger.Flush();

        }
    }
}
