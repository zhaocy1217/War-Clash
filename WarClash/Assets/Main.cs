﻿using Logic;
using Logic.LogicObject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour {

    public Transform uiparent;
    public static Main inst;
    private U3DSceneManager u3dSceneManager;
    private ManagerDriver managerDriver;
    void Awake()
    {
        managerDriver = new ManagerDriver();
        inst = this;
    }
	// Use this for initialization
	void Start ()
    {
        DonotDestroy();
        Logic.LogicCore.SP.Init();
        u3dSceneManager = new U3DSceneManager();
        Logic.LogicCore.SP.sceneManager.SwitchScene(new Scene());
        //Resource.Load("Farm01-flower-03.png", (path, obj) => 
        //{
        //   // GameObject g = GameObject.Instantiate(obj) as GameObject;
        //    //g.transform.parent = uiparent;

        //});
       
        //   EventDispatcher.FireEvent((int)EventList.PreLoadResource, this, EventGroup.NewArg<EventSingleArgs<string>, string>("Prefabs/footman_prefab"));
        //var npc1 = LogicCore.SP.sceneManager.currentScene.CreateSceneObject<Npc>();
        //npc1.Position = new Lockstep.Vector3d(new Vector3(-10, 0, 0));
        //var npc2 = LogicCore.SP.sceneManager.currentScene.CreateSceneObject<Npc>();
        //npc2.Position = new Lockstep.Vector3d(new Vector3(10, 0, 0));

        //npc.ReleaseSkill(Application.streamingAssetsPath+"/skills/1.skill");
    }
    void DonotDestroy()
    {
        var objs = GameObject.FindGameObjectsWithTag("NotDestroy");
        for (int i = 0; i < objs.Length; i++)
        {
            DontDestroyOnLoad(objs[i]);
        }
    }
    float ?t = 0;
	// Update is called once per frame
	void Update () {
        managerDriver.Update();
        u3dSceneManager.Update();
        Logic.LogicCore.SP.Update(Time.deltaTime);
        if(Time.time>5 && t!=null)
        {
            Resource.Load("UI/NextBattleUI.prefab", (path, obj) =>
            {
                GameObject g = GameObject.Instantiate(obj) as GameObject;
                g.transform.parent = uiparent;

            });
            t = null;
        }
       
	}
    void FixedUpdate()
    {
        for (int i = 0; i < 1; i++)
        {
            Logic.LogicCore.SP.FixedUpdate();
        }
    }
}
