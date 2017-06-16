/*
 * Copyright (c) 2016 Robert Adams
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
/*
 * Some code covered by: Copyright (c) Contributors, http://opensimulator.org/
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace org.herbal3d.convoar {
    public class ConvoarParams {
        private static string _logHeader = "[CONVOAR PARAMS]";

        public ConvoarParams() {
            SetParameterDefaultValues();
        }

#pragma warning disable CS0649  // disable 'never assigned' warnings
        public string InputOAR;
        public string OutputDirectory;
        public string Displacement;
        public string Rotation;

        public bool MergeStaticMeshes;      // whether to merge meshes with similar materials
        public bool MergeNonStaticMeshes;      // whether to merge meshes with non-static entities

        public string GltfTargetDir;    // where to store all the Gltf files
        public bool ExportTextures;     // also export textures to the target dir
        public int MaxTextureSize;      // the maximum pixel dimension for images if exporting
        public bool AddTerrainMesh;     // whether to create and add a terrain mesh
        public bool CreateTerrainSplat; // whether to generate a terrain mesh splat texture

        public int VerticesMaxForBuffer;    // Number of vertices to cause splitting of buffer files
        public bool HalfRezTerrain;     // whether to reduce the terrain resolution by 2

        public bool DisplayTimeScaling; // 'true' if to delay mesh scaling to display/GPU time

        public string URIBase;          // the URI base to be added to the beginning of the asset name

        public bool UseOpenSimImageDecoder; //  Use the OpenSimulator image decoder to process JPEG2000 images

        public bool LogConversionStats; // output numbers about number of entities converted
        public bool LogDetailedSharedFaceStats; // output numbers about face mesh sharing
        public bool LogDetailedEntityInfo;      // output detailed information about each entity
        #pragma warning restore CS0649

        // =====================================================================================
        // =====================================================================================
        // List of all of the externally visible parameters.
        // For each parameter, this table maps a text name to getter and setters.
        // To add a new externally referencable/settable parameter, add the paramter storage
        //    location somewhere in the program and make an entry in this table with the
        //    getters and setters.
        // It is easiest to find an existing definition and copy it.
        //
        // A ParameterDefn<T>() takes the following parameters:
        //    -- the text name of the parameter. This is used for console input and ini file.
        //    -- a short text description of the parameter. This shows up in the console listing.
        //    -- a default value
        //    -- a delegate for getting the value
        //    -- a delegate for setting the value
        //    -- an optional delegate to update the value in the world. Most often used to
        //          push the new value to an in-world object.
        //
        // The single letter parameters for the delegates are:
        //    v = value (appropriate type)
        private ParameterDefnBase[] ParameterDefinitions =
        {
            new ParameterDefn<string>("InputOAR", "The input OAR file",
                null),
            new ParameterDefn<string>("OutputDirectory", "The directory (relative to simulator) to hold Basil assets",
                "./BasilAssets", "d" ),
            new ParameterDefn<string>("Displacement", "Optional displacement to add to OAR entites",
                null ),
            new ParameterDefn<string>("Rotation", "Optional rotation to add to OAR entites",
                null ),

            new ParameterDefn<bool>("MergeStaticMeshes", "whether to merge meshes with similar materials",
                true ),
            new ParameterDefn<bool>("MergeNonStaticMeshes", "whether to merge meshes within non-static entities ",
                true ),
            new ParameterDefn<string>("GltfTargetDir", "Where to store all the Gltf files",
                "./gltf" ),
            new ParameterDefn<bool>("ExportTextures", "Convert textures to PNGs and export to target dir",
                true ),
            new ParameterDefn<int>("MaxTextureSize", "The maximum pixel dimension for images if exporting",
                256 ),
            new ParameterDefn<bool>("AddTerrainMesh", "whether to create and add a terrain mesh",
                true ),
            new ParameterDefn<bool>("CreateTerrainSplat", "whether to generate a terrain mesh splat texture",
                true ),
            new ParameterDefn<int>("VerticesMaxForBuffer", "Number of vertices to cause splitting of buffer files",
                50000 ),
            new ParameterDefn<bool>("HalfRezTerrain", "Whether to reduce the terrain resolution by 2",
                false ),
            new ParameterDefn<bool>("DisplayTimeScaling", "If to delay mesh scaling to display/GPU time",
                false ),
            new ParameterDefn<string>("URIBase", "the string added to be beginning of asset name to create URI",
                "./" ),
            new ParameterDefn<bool>("UseOpenSimImageDecoder", "Use the OpenSimulator image decoder to process JPEG2000 images",
                false ),
            new ParameterDefn<bool>("LogConversionStats", "output numbers about number of entities converted",
                true ),
            new ParameterDefn<bool>("LogDetailedSharedFaceStats", "output numbers about face mesh sharing",
                true ),
            new ParameterDefn<bool>("LogDetailedEntityInfo", "output detailed information about each entity",
                false ),
        };

        // =====================================================================================
        // =====================================================================================

        // Base parameter definition that gets and sets parameter values via a string
        public abstract class ParameterDefnBase {
            public string name;         // string name of the parameter
            public string desc;         // a short description of what the parameter means
            public string[] symbols;    // command line symbols for this parameter (short forms)
            public ConvoarParams context; // context for setting and getting values
            public ParameterDefnBase(string pName, string pDesc, string[] pSymbols) {
                name = pName;
                desc = pDesc;
                symbols = pSymbols;
            }
            // Set the parameter value to the default
            public abstract void AssignDefault();
            // Get the value as a string
            public abstract string GetValue();
            // Set the value to this string value
            public abstract void SetValue(string valAsString);
        }

        // Specific parameter definition for a parameter of a specific type.
        public delegate T PGetValue<T>();
        public delegate void PSetValue<T>(T val);
        public sealed class ParameterDefn<T> : ParameterDefnBase {
            private T defaultValue;
            private PSetValue<T> setter;
            private PGetValue<T> getter;
            public ParameterDefn(string pName, string pDesc, T pDefault, PGetValue<T> pGetter, PSetValue<T> pSetter, params string[] symbols)
                : base(pName, pDesc, symbols) {
                defaultValue = pDefault;
                setter = pSetter;
                getter = pGetter;
            }
            // Simple parameter variable where property name is the same as the INI file name
            //     and the value is only a simple get and set.
            public ParameterDefn(string pName, string pDesc, T pDefault, params string[] symbols)
                : base(pName, pDesc, symbols) {
                defaultValue = pDefault;
                setter = (v) => { SetValueByName(name, v); };
                getter = () => { return GetValueByName(name); };
            }
            // Use reflection to find the property named 'pName' in Param and assign 'val' to same.
            private void SetValueByName(string pName, T val) {
                FieldInfo prop = context.GetType().GetField(pName);
                if (prop == null) {
                    // This should only be output when someone adds a new INI parameter and misspells the name.
                    // m_log.ErrorFormat("{0} SetValueByName: did not find '{1}'. Verify specified property name is the same as the given INI parameters name.", LogHeader, pName);
                    System.Console.WriteLine("{0} SetValueByName: did not find '{1}'. Verify specified field name is the same as the given INI parameters name.", _logHeader, pName);
                }
                else {
                    prop.SetValue(context, val);
                }
            }
            // Use reflection to find the property named 'pName' in Param and return the value in same.
            private T GetValueByName(string pName)
            {
                FieldInfo prop = context.GetType().GetField(pName);
                if (prop == null) {
                    // This should only be output when someone adds a new INI parameter and misspells the name.
                    // m_log.ErrorFormat("{0} GetValueByName: did not find '{1}'. Verify specified property name is the same as the given INI parameter name.", LogHeader, pName);
                    System.Console.WriteLine("{0} GetValueByName: did not find '{1}'. Verify specified field name is the same as the given INI parameter name.", _logHeader, pName);
                }
                return (T)prop.GetValue(context);
            }
            public override void AssignDefault() {
                setter(defaultValue);
            }
            public override string GetValue() {
                return getter().ToString();
            }
            public override void SetValue(string valAsString) {
                // Get the generic type of the setter
                Type genericType = setter.GetType().GetGenericArguments()[0];
                // Find the 'Parse' method on that type
                System.Reflection.MethodInfo parser = null;
                try {
                    parser = genericType.GetMethod("Parse", new Type[] { typeof(String) } );
                }
                catch (Exception e) {
                    System.Console.WriteLine("{0} Exception getting parser for type '{1}': {2}", _logHeader, genericType, e);
                    parser = null;
                }
                if (parser != null) {
                    // Parse the input string
                    try {
                        T setValue = (T)parser.Invoke(genericType, new Object[] { valAsString });
                        // Store the parsed value
                        setter(setValue);
                        // // m_log.DebugFormat("{0} Parameter {1} = {2}", LogHeader, name, setValue);
                    }
                    catch {
                        // m_log.ErrorFormat("{0} Failed parsing parameter value '{1}' as type '{2}'", LogHeader, valAsString, genericType);
                    }
                }
                else {
                    // m_log.ErrorFormat("{0} Could not find parameter parser for type '{1}'", LogHeader, genericType);
                }
            }
        }

        // Search through the parameter definitions and return the matching
        //    ParameterDefn structure.
        // Case does not matter as names are compared after converting to lower case.
        // Returns 'false' if the parameter is not found.
        public bool TryGetParameter(string paramName, out ParameterDefnBase defn) {
            bool ret = false;
            ParameterDefnBase foundDefn = null;
            string pName = paramName.ToLower();

            foreach (ParameterDefnBase parm in ParameterDefinitions) {
                string parmL = parm.name.ToLower();
                if (pName == parmL) {
                    foundDefn = parm;
                    ret = true;
                }
                if (parm.symbols != null) {
                    foreach (string sym in parm.symbols) {
                        if (sym == parmL) {
                            foundDefn = parm;
                            ret = true;
                            break;
                        }
                    }
                }
                if (ret) break;
            }
            defn = foundDefn;
            return ret;
        }

        // Find the named parameter and set its value.
        // Returns 'false' if the parameter could not be found.
        public bool SetParameterValue(string paramName, string valueAsString) {
            bool ret = false;
            ParameterDefnBase parm;
            if (TryGetParameter(paramName, out parm)) {
                parm.SetValue(valueAsString);
                ret = true;
            }
            return ret;
        }

        // Pass through the settable parameters and set the default values.
        public void SetParameterDefaultValues() {
            foreach (ParameterDefnBase parm in ParameterDefinitions) {
                parm.context = this;
                parm.AssignDefault();
            }
        }

        public void MergeCommandLine(string[] args) {
            MergeCommandLine(args, null, null);
        }

        // Given parameters from the command line, read the parameters and set values specified
        // <param name="args">array of command line tokens</param>
        // <param name="firstOpFlag">if 'true' presume the first token in the parameter line
        // is a special value that should be assigned to the keyword "--firstparam".</param>
        // <param name="multipleLastParameters">if 'true' presume multiple specs at the end of the line
        // are filenames and pack them together into a CSV string in LAST_PARAM.</param>
        public bool MergeCommandLine(string[] args, string firstOpParameter, string multipleLastParameters) {
            bool ret = true;    // start out assuming parsing worked

            bool firstOpFlag = false;   // no first op
            if (!String.IsNullOrEmpty(firstOpParameter)) {
                firstOpFlag = true;
            }
            bool multipleLast = false;
            if (!String.IsNullOrEmpty(multipleLastParameters)) {
                multipleLast = true;
            }


            for (int ii = 0; ii < args.Length; ii++) {
                string para = args[ii];
                // is this a parameter?
                if (para[0] == '-') {
                    // is the next one a parameter?
                    if (ii == (args.Length - 1) || args[ii + 1][0] == '-') {
                        // two parameters in a row. this must be a toggle parameter
                        AddCommandLineParameter(para, null);
                    }
                    else {
                        // looks like a parameter followed by a value
                        AddCommandLineParameter(para, args[ii + 1]);
                        ii++;       // skip the value we just added to the dictionary
                    }
                }
                else {
                    if (ii == 0 && firstOpFlag) {
                        // if the first thing is not a parameter, make like it's an op or something
                        AddCommandLineParameter(firstOpParameter, args[ii + 1]);
                    }
                    else {
                        if (multipleLast) {
                            // Pack all remaining arguments into a comma-separated list as LAST_PARAM
                            StringBuilder multFiles = new StringBuilder();
                            for (int jj = ii; jj < args.Length; jj++) {
                                if (multFiles.Length != 0) {
                                    multFiles.Append(",");
                                }
                                multFiles.Append(args[jj]);
                            }
                            AddCommandLineParameter(multipleLastParameters, multipleLast.ToString());

                            // Skip them all
                            ii = args.Length;
                        }
                        else {
                            throw new ArgumentException("Unknown parameter " + para);
                        }
                    }
                }
            }

            return ret;
        }

        private bool AddCommandLineParameter(string parm, string val) {
            bool ret = false;
            ParameterDefnBase parmDefn;
            if (TryGetParameter(parm, out parmDefn)) {
                parmDefn.SetValue(val);
                ret = true;
            }
            return ret;
        }
    }
}
