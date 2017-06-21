﻿/*
 * Copyright (c) 2017 Robert Adams
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OMV = OpenMetaverse;

namespace org.herbal3d.convoar {
    /// <summary>
    /// A set of classes that hold viewer displayable items. These can be
    /// meshes, procedures, or whatever.
    /// </summary>
    public class Displayable {
        public OMV.Vector3 offsetPosition;
        public OMV.Quaternion offsetRotation;
        public OMV.Vector3 scale;
        public DisplayableRenderable renderable;
        public List<Displayable> children;

        public Displayable() {
            offsetPosition = OMV.Vector3.Zero;
            offsetRotation = OMV.Quaternion.Identity;
            scale = new OMV.Vector3(1, 1, 1);
            renderable = null;
            children = new List<Displayable>();
        }

        public Displayable(DisplayableRenderable pRenderable) {
            offsetPosition = OMV.Vector3.Zero;
            offsetRotation = OMV.Quaternion.Identity;
            scale = new OMV.Vector3(1, 1, 1);
            renderable = pRenderable;
            children = new List<Displayable>();
        }
    }

    /// <summary>
    /// The parent class of the renderable parts of the displayable.
    /// Could be a mesh or procedure or whatever.
    /// </summary>
    public abstract class DisplayableRenderable {
        public Object userData;   // usually a reference to the source object when creating the Renderable
    }

    /// <summary>
    /// A group of meshes that make up a renderable item
    /// </summary>
    public class RenderableMeshGroup : DisplayableRenderable {
        public List<RenderableMesh> meshes; // The meshes that make up this Renderable
    }
        
    public class RenderableMesh {
        public int num;                 // number of this face on the prim
        public EntityHandle mesh;
        public EntityHandle material;
    }



} 