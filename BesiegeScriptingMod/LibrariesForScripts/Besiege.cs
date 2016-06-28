using System;
using System.Collections.Generic;
using System.Linq;
using spaar.ModLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BesiegeScriptingMod.LibrariesForScripts
{
    public class Besiege
    {
        public AddPiece addPiece;
        public List<GameObject> prefabs;
        public List<GameObject> ghosts; 
        public LEVELLORD levellord;
        public GameObject mainCamera;
        public PrefabMaster prefabMaster;
        public StatMaster statMaster;
        public InputManager inputManager;
        public OptionsMaster optionsMaster;
        public ReferenceMaster referenceMaster;
        public SaveOptions saveOptions;
        public BlockSkinLoader blockSkinLoader;
        public AssetImporter assetImporter;
        public SteamManager steamManager;
        public WorkshopManager workkshopManager;
        public ListOfNames listOfNames;
        public MachineObjectTracker machineObjectTracker;
        public Machine machine;
        public GameObject buildingMachine;
        public GameObject simulationMachine;

        internal static Besiege _besiege;

        internal static void SetUp()
        {
            if (!Settings.useAPI)
            {
                return;
            }
            Game.OnSimulationToggle += GameOnOnSimulationToggle;
            var p = GameObject.Find("_PERSISTENT");
            Besiege besiege = new Besiege
            {
                addPiece = Object.FindObjectOfType<AddPiece>(),
                prefabs = new List<GameObject>(),
                levellord = Object.FindObjectOfType<LEVELLORD>(),
                mainCamera = GameObject.Find("Main Camera"),
                prefabMaster = p.GetComponent<PrefabMaster>(),
                statMaster = p.GetComponent<StatMaster>(),
                inputManager = p.GetComponent<InputManager>(),
                optionsMaster = p.GetComponent<OptionsMaster>(),
                referenceMaster = p.GetComponent<ReferenceMaster>(),
                saveOptions = p.GetComponent<SaveOptions>(),
                blockSkinLoader = p.GetComponent<BlockSkinLoader>(),
                assetImporter = p.GetComponent<AssetImporter>(),
                steamManager = p.GetComponent<SteamManager>(),
                workkshopManager = p.GetComponent<WorkshopManager>(),
                listOfNames = p.GetComponent<ListOfNames>(),
            };
            foreach (Transform transform in p.transform.FindChild("BLOCKS").FindChild("Prefabs"))
            {
                besiege.prefabs.Add(transform.gameObject);
            }
            besiege.ghosts = new List<GameObject>();
            foreach (Transform component in p.transform.FindChild("BLOCKS").FindChild("Ghosts"))
            {
                besiege.ghosts.Add(component.gameObject);
            }
            if (Application.loadedLevel != 1)
            {
                besiege.buildingMachine = besiege.machine.gameObject.transform.FindChild("Building Machine").gameObject;
                besiege.machine = Object.FindObjectOfType<Machine>();
                besiege.machineObjectTracker = Object.FindObjectOfType<MachineObjectTracker>();
                if (Game.IsSimulating)
                {
                    besiege.simulationMachine = besiege.machine.gameObject.transform.FindChild("Simulation Machine").gameObject;
                }
            }
            Besiege._besiege = besiege;
        }

        private static void GameOnOnSimulationToggle(bool simulating)
        {
            if (simulating)
            {
                _besiege.simulationMachine = _besiege.machine.gameObject.transform.FindChild("Simulation Machine").gameObject;
            }
            else
            {
                _besiege.simulationMachine = null;
            }
        }

        internal static void OnLevelWasLoaded()
        {
            if (!Settings.useAPI)
            {
                return;
            }
            var p = GameObject.Find("_PERSISTENT");
            Besiege besiege = new Besiege
            {
                addPiece = Object.FindObjectOfType<AddPiece>(),
                prefabs = new List<GameObject>(),
                levellord = Object.FindObjectOfType<LEVELLORD>(),
                mainCamera = GameObject.Find("Main Camera"),
                prefabMaster = p.GetComponent<PrefabMaster>(),
                statMaster = p.GetComponent<StatMaster>(),
                inputManager = p.GetComponent<InputManager>(),
                optionsMaster = p.GetComponent<OptionsMaster>(),
                referenceMaster = p.GetComponent<ReferenceMaster>(),
                saveOptions = p.GetComponent<SaveOptions>(),
                blockSkinLoader = p.GetComponent<BlockSkinLoader>(),
                assetImporter = p.GetComponent<AssetImporter>(),
                steamManager = p.GetComponent<SteamManager>(),
                workkshopManager = p.GetComponent<WorkshopManager>(),
                listOfNames = p.GetComponent<ListOfNames>(),
                machine = Object.FindObjectOfType<Machine>(),
                machineObjectTracker = Object.FindObjectOfType<MachineObjectTracker>()
            };
            besiege.buildingMachine = besiege.machine.gameObject.transform.FindChild("Building Machine").gameObject;
            foreach (Transform transform in p.transform.FindChild("BLOCKS").FindChild("Prefabs"))
            {
                besiege.prefabs.Add(transform.gameObject);
            }
            besiege.ghosts = new List<GameObject>();
            foreach (Transform component in p.transform.FindChild("BLOCKS").FindChild("Ghosts"))
            {
                besiege.ghosts.Add(component.gameObject);
            }
            if (Game.IsSimulating)
            {
                besiege.simulationMachine = besiege.machine.gameObject.transform.FindChild("Simulation Machine").gameObject;
            }
            Besiege._besiege = besiege;
        }

        public BlockBehaviour GetBlockByGuid(String GUID)
        {
            return _besiege.machine.Blocks.FirstOrDefault(blockBehaviour => blockBehaviour.Guid.Equals(new Guid(GUID)));
        }

        public BlockBehaviour GetBlockByID(int id)
        {
            return _besiege.machine.Blocks.FirstOrDefault(t => t.GetBlockID() == id);
        }

        public BlockBehaviour GetBlockByName(String name)
        {
            return _besiege.machine.Blocks.FirstOrDefault(t => t.name == name);
        }
    }
}
