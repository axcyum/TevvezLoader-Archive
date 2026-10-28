using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VRClient.IMGUI;
using VRClient.Modules;

namespace VRClient.Menu
{
    internal class CamUI : UiModule.Page
    {
        public CamUI() : base("Cam") { }

        public override void Render()
        {
            GUILayout.BeginVertical();



            GUILayout.EndVertical();
        }
    }

    internal class CamModule : BaseModule
    {
        public override void Awake()
        {
            base.Awake();
        }

        public override void OnUpdate()
        {
            ThirdPerson.Update();
        }
    }
}
