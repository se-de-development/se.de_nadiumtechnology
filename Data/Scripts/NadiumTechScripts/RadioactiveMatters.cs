using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace Radiation{
	
	[MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
		
	public class RadioactiveMatters : MySessionComponentBase{
		
		int tickTimer = 0;
		bool scriptInit = false;
		
		MyObjectBuilder_PhysicalObject composite_plate;
		MyObjectBuilder_PhysicalObject radioactive_nadium;
		MyObjectBuilder_PhysicalObject electromagnetic_element;
		
		public override void UpdateBeforeSimulation(){
			
			if(MyAPIGateway.Multiplayer.IsServer == false){
				
				return;
				
			}
			
			if(scriptInit == false){
				
				scriptInit = true;
				
				
				var definitionId = new MyDefinitionId(typeof(MyObjectBuilder_Component), "Nadium_Radioactive");
				radioactive_nadium = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(definitionId);
				
				definitionId = new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Unknown_Element");
				electromagnetic_element = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(definitionId);
				
				definitionId = new MyDefinitionId(typeof(MyObjectBuilder_Component), "composite_plate");
				composite_plate = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(definitionId);
			
			}
			
			tickTimer++;
			
			if(tickTimer < 180){
				
				return;
				
			}
			
			tickTimer = 0;
			
			var playerList = new List<IMyPlayer>();
			MyAPIGateway.Players.GetPlayers(playerList);
			
			if(playerList.Count == 0){
				
				return;
				
			}
			
			foreach(var player in playerList){
				
				if(player.IsBot == true){
					
					continue;
					
				}
				
				if(player.Character == null){
					
					continue;
					
				}
				
				float health = MyVisualScriptLogicProvider.GetPlayersHealth(player.IdentityId);
				float energy = MyVisualScriptLogicProvider.GetPlayersEnergyLevel(player.IdentityId);
				
				if(health <= 0){
					
					continue;
					
				}
			

				var playerInv = player.Character.GetInventory();
				MyFixedPoint amount = playerInv.GetItemAmount(electromagnetic_element);
				MyFixedPoint composite_plate_amount = playerInv.GetItemAmount(composite_plate);
			
					if(playerInv.ContainItems(1, radioactive_nadium) == true){

						MyVisualScriptLogicProvider.SetPlayersHealth(player.IdentityId, health - 5);
						health = MyVisualScriptLogicProvider.GetPlayersHealth(player.IdentityId);
						MyVisualScriptLogicProvider.ShowNotification(">>> WARNING RADIATION ALERT <<<\n\nToxic Nadium radiation detected\nIntensity: 2,5 Rad/s", 2000, "Red", player.IdentityId);
						//MyVisualScriptLogicProvider.ShowNotificationToAll("WARNING RADIATION ALERT\n\nDetected toxic substance\n\nTriNadiumTaloxid-438", 2000, "White");
					}
					if(playerInv.ContainItems(1, composite_plate) == true){

						playerInv.RemoveItemsOfType(composite_plate_amount, composite_plate);
						MyVisualScriptLogicProvider.SetPlayersHealth(player.IdentityId, health - 50);
						health = MyVisualScriptLogicProvider.GetPlayersHealth(player.IdentityId);
						MyVisualScriptLogicProvider.ShowNotification(">>> WARNING RADIATION ALERT <<<\n\nUnknown radiation detected\nIntensity: 50 Rad/s", 2000, "Red", player.IdentityId);
					}
					
					if(playerInv.ContainItems(1, electromagnetic_element) == true){
						if(energy <= 0){
						MyVisualScriptLogicProvider.SetPlayersEnergyLevel(player.IdentityId, 0);
						}
						else{
						MyVisualScriptLogicProvider.SetPlayersEnergyLevel(player.IdentityId, energy - .04f);
						playerInv.RemoveItemsOfType(amount, electromagnetic_element);
						energy = MyVisualScriptLogicProvider.GetPlayersEnergyLevel(player.IdentityId);
						MyVisualScriptLogicProvider.ShowNotification(">>> INTERNAL ENERGY LOSS DETECTED <<<", 2000, "White", player.IdentityId);
						}
					}
				
			}
		}	
	}	
}