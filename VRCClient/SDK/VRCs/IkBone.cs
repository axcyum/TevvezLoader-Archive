using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VRClient.SDK.VRCs
{
    public class IkBone
    {
        public Transform bone;
        public List<IkBone> children = new List<IkBone>();

        public IkBone(Transform bone) => this.bone = bone;

        public IkBone WithChild(IkBone child)
        {
            if (child?.bone != null)
                children.Add(child);
            return this;
        }

        public void CollectLines(List<(Transform, Transform)> lines)
        {
            foreach (var child in children)
            {
                if (bone != null && child.bone != null)
                    lines.Add((bone, child.bone));
                child.CollectLines(lines);
            }
        }
    }
}
