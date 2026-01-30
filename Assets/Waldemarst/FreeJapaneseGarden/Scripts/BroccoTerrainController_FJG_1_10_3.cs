using System.Collections.Generic;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Broccoli.Controller {
	/// <summary>
	/// Controls a Tree Broccoli Instances in Terrains.
	/// </summary>
	public class BroccoTerrainController_FJG_1_10_3 : MonoBehaviour {
		#region Vars
        /// <summary>
        /// Terrain component.
        /// </summary>
        Terrain terrain = null;
        /// <summary>
        /// Keeps track of the material instances to update.
        /// </summary>
        List<Material> _mats = new List<Material> ();
		/// <summary>
		/// Keeps track of the material instances parameters.
		/// for BroccoTreeController:
		/// x: sproutTurbulance.
		/// y: sproutSway.
		/// z: localWindAmplitude.
		/// for BroccoTreeController_FJG_1_10_3:
		/// x: trunkBending.
		/// </summary>
		List<Vector3> _matParams = new List<Vector3> ();
		/// <summary>
		/// Materials array.
		/// Materials are taken from BroccoTreeController_FJG_1_10_3.
		/// </summary>
		Material[] broccoMaterials;
		/// <summary>
		/// Material params array.
		/// Parameters are taken from BroccoTreeControllers2.
		/// </summary>
		Vector3[] broccoMaterialParams;
		bool requiresUpdateWindZoneValues = true;
		private float baseWindAmplitude = 0.2752f;
		private float windGlobalW = 1.728f;
		public static float globalWindAmplitude = 1f;
		public float valueWindMain = 0f;
		public float valueWindTurbulence = 0f;
		public Vector3 valueWindDirection = Vector3.zero;
		private float valueLeafSwayFactor = 1f;
		private float valueLeafTurbulenceFactor = 1f;
		private int _frameCount = -1;
		#endregion

		#region Shader values
		float valueTime = 0f;
		float valueTimeWindMain = 0f;
		float windTimeScale = 1f;
		Vector4 valueSTWindVector = Vector4.zero;
		Vector4 valueSTWindGlobal = Vector4.zero;
		Vector4 valueSTWindBranch = Vector4.zero;
		Vector4 valueSTWindBranchTwitch = Vector4.zero;
		Vector4 valueSTWindBranchWhip = Vector4.zero;
		Vector4 valueSTWindBranchAnchor = Vector4.zero;
		Vector4 valueSTWindBranchAdherences = Vector4.zero;
		Vector4 valueSTWindTurbulences = Vector4.zero;
		Vector4 valueSTWindLeaf1Ripple = Vector4.zero;
		Vector4 valueSTWindLeaf1Tumble = Vector4.zero;
		Vector4 valueSTWindLeaf1Twitch = Vector4.zero;
		Vector4 valueSTWindLeaf2Ripple = Vector4.zero;
		Vector4 valueSTWindLeaf2Tumble = Vector4.zero;
		Vector4 valueSTWindLeaf2Twitch = Vector4.zero;
		Vector4 valueSTWindFrondRipple = Vector4.zero;
		Vector4 value2STWindVector = Vector4.zero;
		Vector4 value2STWindGlobal = Vector4.zero;
		Vector4 value2STWindBranch = Vector4.zero;
		Vector4 value2STWindBranchTwitch = Vector4.zero;
		Vector4 value2STWindBranchWhip = Vector4.zero;
		Vector4 value2STWindBranchAnchor = Vector4.zero;
		Vector4 value2STWindBranchAdherences = Vector4.zero;
		Vector4 value2STWindTurbulences = Vector4.zero;
		Vector4 value2STWindLeaf1Ripple = Vector4.zero;
		Vector4 value2STWindLeaf1Tumble = Vector4.zero;
		Vector4 value2STWindLeaf1Twitch = Vector4.zero;
		Vector4 value2STWindLeaf2Ripple = Vector4.zero;
		Vector4 value2STWindLeaf2Tumble = Vector4.zero;
		Vector4 value2STWindLeaf2Twitch = Vector4.zero;
		Vector4 value2STWindFrondRipple = Vector4.zero;
		#endregion

		#region Shader Property Ids
		static int propWindEnabled = 0;
		static int propWindQuality = 0;
		static int propSTWindVector = 0;
		static int propSTWindGlobal = 0;
		static int propSTWindBranch = 0;
		static int propSTWindBranchTwitch = 0;
		static int propSTWindBranchWhip = 0;
		static int propSTWindBranchAnchor = 0;
		static int propSTWindBranchAdherences = 0;
		static int propSTWindTurbulences = 0;
		static int propSTWindLeaf1Ripple = 0;
		static int propSTWindLeaf1Tumble = 0;
		static int propSTWindLeaf1Twitch = 0;
		static int propSTWindLeaf2Ripple = 0;
		static int propSTWindLeaf2Tumble = 0;
		static int propSTWindLeaf2Twitch = 0;
		static int propSTWindFrondRipple = 0;
		#endregion

		#region Static Constructor
        /// <summary>
        /// Static controller for this class.
        /// </summary>
		static BroccoTerrainController_FJG_1_10_3 () {
			propWindEnabled = Shader.PropertyToID ("_WindEnabled");
			propWindQuality = Shader.PropertyToID ("_WindQuality");
			propSTWindVector = Shader.PropertyToID ("_ST_WindVector");
			propSTWindVector = Shader.PropertyToID ("_ST_WindVector");
			propSTWindGlobal = Shader.PropertyToID ("_ST_WindGlobal");
			propSTWindBranch = Shader.PropertyToID ("_ST_WindBranch");
			propSTWindBranchTwitch = Shader.PropertyToID ("_ST_WindBranchTwitch");
			propSTWindBranchWhip = Shader.PropertyToID ("_ST_WindBranchWhip");
			propSTWindBranchAnchor = Shader.PropertyToID ("_ST_WindBranchAnchor");
			propSTWindBranchAdherences = Shader.PropertyToID ("_ST_WindBranchAdherences");
			propSTWindTurbulences = Shader.PropertyToID ("_ST_WindTurbulences");
			propSTWindLeaf1Ripple = Shader.PropertyToID ("_ST_WindLeaf1Ripple");
			propSTWindLeaf1Tumble = Shader.PropertyToID ("_ST_WindLeaf1Tumble");
			propSTWindLeaf1Twitch = Shader.PropertyToID ("_ST_WindLeaf1Twitch");
			propSTWindLeaf2Ripple = Shader.PropertyToID ("_ST_WindLeaf2Ripple");
			propSTWindLeaf2Tumble = Shader.PropertyToID ("_ST_WindLeaf2Tumble");
			propSTWindLeaf2Twitch = Shader.PropertyToID ("_ST_WindLeaf2Twitch");
			propSTWindFrondRipple = Shader.PropertyToID ("_ST_WindFrondRipple");
		}
		#endregion

		#region Events
		/// <summary>
		/// Start this instance.
		/// </summary>
		public void Start () {
            // Get the terrain.
            terrain = GetComponent<Terrain> ();
            if (terrain != null) {
				requiresUpdateWindZoneValues = true;
				SetupWind ();
            }
		}
		/// <summary>
		/// Update this instance.
		/// </summary>
		void Update () {
			#if UNITY_EDITOR
			if (EditorApplication.isPlaying && _frameCount != Time.frameCount) {
				valueTime = (EditorApplication.isPlaying)?Time.time:(float)EditorApplication.timeSinceStartup;
				valueTime *= windTimeScale;
				valueTimeWindMain = valueTime * 0.66f;
				for (int i = 0; i < broccoMaterials.Length; i++) {
					UpdateBroccoTreeWind (broccoMaterials [i], broccoMaterialParams [i]);
				}
				_frameCount = Time.frameCount;
			}
			#else
			if (_frameCount != Time.frameCount) {
				valueTime = Time.time;
				valueTime *= windTimeScale;
				valueTimeWindMain = valueTime * 0.66f;
				for (int i = 0; i < broccoMaterials.Length; i++) {
					UpdateBroccoTreeWind (broccoMaterials [i], broccoMaterialParams [i]);
				}
				_frameCount = Time.frameCount;
			}
			#endif
		}
		#endregion

		#region Wind
		public void UpdateWind (float windMain, float windTurbulence, Vector3 windDirection) {
			valueWindMain = windMain;
			valueWindTurbulence = windTurbulence;
			valueWindDirection = windDirection;
			for (int i = 0; i < broccoMaterials.Length; i++) {
				ApplyBroccoTreeWind (broccoMaterials [i], broccoMaterialParams [i]);
				UpdateBroccoTreeWind (broccoMaterials [i], broccoMaterialParams [i]);
			}
		}
        /// <summary>
        /// Setup the wind on Tree Prototype materials found on this terrain
        /// and add their materials to an array to update the wind on each frame.
        /// </summary>
		//private void SetupWind (BroccoTreeController treeController) {
		private void SetupWind () {
			// Get all BroccoTreeController materials.
			GameObject treePrefab;
			BroccoTreeController_FJG_1_10_3[] brocco2TreeControllers;

			// Get all BroccoTreeController_FJG_1_10_3 materials for Terrain Tree Prototypes.
			for (int i = 0; i < terrain.terrainData.treePrototypes.Length; i++) {
				treePrefab = terrain.terrainData.treePrototypes [i].prefab;
				if (treePrefab != null) {
					brocco2TreeControllers = treePrefab.GetComponentsInChildren<BroccoTreeController_FJG_1_10_3> ();
					foreach (BroccoTreeController_FJG_1_10_3 treeController in brocco2TreeControllers) {
						// Setup instances of tree controller according to the controller.
						SetupBrocco2TreeController (treeController);
					}
				}
			}

			// Get all BroccoTreeController_FJG_1_10_3 materials for Terrain Detail Prototypes.
			for (int i = 0; i < terrain.terrainData.detailPrototypes.Length; i++) {
				treePrefab = terrain.terrainData.detailPrototypes [i].prototype;
				if (treePrefab != null) {
					brocco2TreeControllers = treePrefab.GetComponentsInChildren<BroccoTreeController_FJG_1_10_3> ();
					foreach (BroccoTreeController_FJG_1_10_3 treeController in brocco2TreeControllers) {
						// Setup instances of tree controller according to the controller.
						SetupBrocco2TreeController (treeController, BroccoTreeController_FJG_1_10_3.WindQuality.Better);
					}
				}
			}


			broccoMaterials = _mats.ToArray ();
			broccoMaterialParams = _matParams.ToArray ();
			_mats.Clear ();
			_matParams.Clear ();
		}
		/// <summary>
        /// Setup materials in instances with BroccoTreeController.
        /// </summary>
        /// <param name="treeController"></param>
        private void SetupBrocco2TreeController (
			BroccoTreeController_FJG_1_10_3 treeController, 
			BroccoTreeController_FJG_1_10_3.WindQuality windQuality = BroccoTreeController_FJG_1_10_3.WindQuality.Best)
		{
            Renderer renderer = treeController.gameObject.GetComponent<Renderer> ();
            Material material;
            if (renderer != null && 
                treeController.localShaderType == BroccoTreeController_FJG_1_10_3.ShaderType.SpeedTree8OrCompatible)
            {
				GetWindZoneValues ();
					
                for (int i = 0; i < renderer.sharedMaterials.Length; i++) {
                    material = renderer.sharedMaterials [i];
                    bool isWindEnabled = windQuality != BroccoTreeController_FJG_1_10_3.WindQuality.None;

                    if (treeController.localShaderType == BroccoTreeController_FJG_1_10_3.ShaderType.SpeedTree8OrCompatible) {
                        material.DisableKeyword ("_WINDQUALITY_NONE");
                        material.DisableKeyword ("_WINDQUALITY_FASTEST");
                        material.DisableKeyword ("_WINDQUALITY_FAST");
                        material.DisableKeyword ("_WINDQUALITY_BETTER");
                        material.DisableKeyword ("_WINDQUALITY_BEST");
                        material.DisableKeyword ("_WINDQUALITY_PALM");
                        if (isWindEnabled) {
                            switch (windQuality) {
                                case BroccoTreeController_FJG_1_10_3.WindQuality.None:
                                    material.EnableKeyword ("_WINDQUALITY_NONE");
                                    break;
                                case BroccoTreeController_FJG_1_10_3.WindQuality.Fastest:
                                    material.EnableKeyword ("_WINDQUALITY_FASTEST");
                                    break;
                                case BroccoTreeController_FJG_1_10_3.WindQuality.Fast:
                                    material.EnableKeyword ("_WINDQUALITY_FAST");
                                    break;
                                case BroccoTreeController_FJG_1_10_3.WindQuality.Better:
                                    material.EnableKeyword ("_WINDQUALITY_BETTER");
                                    break;
                                case BroccoTreeController_FJG_1_10_3.WindQuality.Best:
                                    material.EnableKeyword ("_WINDQUALITY_BEST");
                                    break;
                                case BroccoTreeController_FJG_1_10_3.WindQuality.Palm:
                                    material.EnableKeyword ("_WINDQUALITY_PALM");
                                    break;
                            }
                        }
                    } else if (isWindEnabled) {
                        if (windQuality != BroccoTreeController_FJG_1_10_3.WindQuality.None) {
                            material.EnableKeyword ("ENABLE_WIND");
                        } else {
                            material.DisableKeyword ("ENABLE_WIND");
                        }
                    }
                    // Set the material wind properties that don't change with wind updates.
                    material.SetFloat (propWindEnabled, (isWindEnabled?1f:0f));
                    material.SetFloat (propWindQuality, (float)windQuality);

					// STWindBranchWhip
					value2STWindBranchWhip = new Vector4 (0.0f, 0.0f, 0.0f, 0.0f);
					material.SetVector (propSTWindBranchWhip, value2STWindBranchWhip);
					// STWindBranchAdherences
					value2STWindBranchAdherences = new Vector4 (0.15f, 0.15f, 0f, 0f);
					material.SetVector (propSTWindBranchAdherences, value2STWindBranchAdherences);
					// STWindTurbulences
					value2STWindTurbulences = new Vector4 (0.7f, 0.3f, 0f, 0f);
					material.SetVector (propSTWindTurbulences, value2STWindTurbulences);
					// STWindFrondRipple
					value2STWindFrondRipple = new Vector4 (Time.time * 1f, 0.01f, 2f, 10f);
					material.SetVector (propSTWindFrondRipple, value2STWindFrondRipple);

                    if (!_mats.Contains (material) && isWindEnabled) {
                        _mats.Add (material);
						Vector3 matParams = new Vector3 (treeController.trunkBending, 0f, 0f);
						_matParams.Add (matParams);
						ApplyBroccoTreeWind (material, matParams);
                    }
                }
            }
        }
		private void ApplyBroccoTreeWind (Material mat, Vector3 matParams) {
			// STWindGlobal (time / 2, 0.3, 0.1, 1.7)
			value2STWindGlobal = new Vector4 (0f, 
				baseWindAmplitude * valueWindMain, 
				matParams.x * 0.1f + 0.001f, 
				windGlobalW * 1.125f);
			mat.SetVector (propSTWindGlobal, value2STWindGlobal);

			// STWindBranch (time / 1.5, 0.4f, time * 1.5, 0f)
			value2STWindBranch = new Vector4 (0f, valueWindMain * 0.35f, 0f, 0f);
			mat.SetVector (propSTWindBranch, value2STWindBranch);

			// WIND DIRECTION.
			// STWindVector
			value2STWindVector = valueWindDirection;
			mat.SetVector (propSTWindVector, value2STWindVector);
			// STWindBranchAnchor
			value2STWindBranchAnchor = new Vector4 (
				valueWindDirection.x, 
				valueWindDirection.y, 
				valueWindDirection.z, 
				valueWindMain * 2f);
			mat.SetVector (propSTWindBranchAnchor, value2STWindBranchAnchor);

			// STWindBranchTwitch (AMOUNT, SCALE, 0, 0)
			float branchTwitchAmount = 0.65f - Mathf.Lerp (0f, 0.35f, valueWindTurbulence / 3f);
			value2STWindBranchTwitch = new Vector4 (branchTwitchAmount * branchTwitchAmount, 1, 0f, 0f);
			mat.SetVector (propSTWindBranchTwitch, value2STWindBranchTwitch);

			// STWindLeaf1Tumble (TIME, FLIP, TWIST, ADHERENCE)
			value2STWindLeaf1Tumble = new Vector4 (0f, 
				valueWindTurbulence * 0.1f, 
				//value2WindMain * (value2WindMain>1.5f?0.125f:0.5f), 
				valueWindMain * Mathf.Lerp (0.5f, 0.1f, valueWindMain / 4f),
				valueWindMain * 0.085f);
			// STWindLeaf1Twitch (AMOUNT, SHARPNESS, TIME, 0.0)
			value2STWindLeaf1Twitch = new Vector4 (
				valueWindMain * 0.165f, 
				valueWindTurbulence * 0.165f, 0f, 0f);
			// STWindLeaf1Ripple (TIME, AMOUNT, 0, 0)
			value2STWindLeaf1Ripple = new Vector4 (0f, valueWindTurbulence * 0.01f, 0f, 0f);
			mat.SetVector (propSTWindLeaf1Tumble, value2STWindLeaf1Tumble);
			mat.SetVector (propSTWindLeaf1Twitch, value2STWindLeaf1Twitch);
			mat.SetVector (propSTWindLeaf1Ripple, value2STWindLeaf1Ripple);

			// STWindLeaf2Tumble (TIME, FLIP, TWIST, ADHERENCE)
			value2STWindLeaf2Tumble = new Vector4 (0f, 
				valueWindTurbulence * 0.1f, 
				//value2WindMain * (value2WindMain>1.5f?0.125f:0.5f), 
				valueWindMain * Mathf.Lerp (0.5f, 0.1f, valueWindMain / 4f),
				valueWindMain * 0.085f);
			// STWindLeaf2Twitch (AMOUNT, SHARPNESS, TIME, 0.0)
			value2STWindLeaf2Twitch = new Vector4 (
				valueWindMain * 0.165f, 
				valueWindTurbulence * 0.165f, 0f, 0f);
			// STWindLeaf2Ripple (TIME, AMOUNT, 0, 0)
			value2STWindLeaf2Ripple = new Vector4 (0f, valueWindTurbulence * 0.01f, 0f, 0f);
			mat.SetVector (propSTWindLeaf2Tumble, value2STWindLeaf2Tumble);
			mat.SetVector (propSTWindLeaf2Twitch, value2STWindLeaf2Twitch);
			mat.SetVector (propSTWindLeaf2Ripple, value2STWindLeaf2Ripple);
		}
		/// <summary>
		/// Updates the values of materials.
		/// </summary>
		/// <param name="material">Material</param>
		/// <param name="windParams">Wind parameters, x: sproutTurbulance, y: sproutSway, z: localWindAmplitude.</param>
		private void UpdateBroccoTreeWind (Material material, Vector3 windParams) {
			//_localRenderer.GetPropertyBlock (_propBlock);
			// STWindGlobal
			value2STWindGlobal.x = valueTime * 0.5f;
			value2STWindGlobal.z = windParams.x * 0.1f + 0.001f;

			// STWindBranch (TIME, DISTANCE, 0, 0)
			value2STWindBranch.x = valueTimeWindMain;
			
			// STWindLeaf1Tumble (TIME, FLIP, TWIST, ADHERENCE)
			value2STWindLeaf1Tumble.x = valueTimeWindMain;
			// STWindLeaf1Twitch (AMOUNT, SHARPNESS, TIME, 0.0)
			value2STWindLeaf1Twitch.z = valueTime * 0.5f;
			// STWindLeaf1Ripple (TIME, AMOUNT, 0, 0)
			value2STWindLeaf1Ripple.x = valueTime;
			
			// STWindLeaf2Tumble (TIME, FLIP, TWIST, ADHERENCE)
			value2STWindLeaf2Tumble.x = valueTimeWindMain;
			// STWindLeaf2Twitch (AMOUNT, SHARPNESS, TIME, 0.0)
			value2STWindLeaf2Twitch.z = valueTime * 0.5f;
			// STWindLeaf2Ripple (TIME, AMOUNT, 0, 0)
			value2STWindLeaf2Ripple.x = valueTime;

			material.SetVector (propSTWindGlobal, value2STWindGlobal);
			// STWindBranch
			material.SetVector (propSTWindBranch, value2STWindBranch);

			// STWindLeaf1Tumble (TIME, FLIP, TWIST, ADHERENCE)
			material.SetVector (propSTWindLeaf1Tumble, value2STWindLeaf1Tumble);
			// STWindLeaf1Twitch (AMOUNT, SHARPNESS, TIME, 0.0)
			material.SetVector (propSTWindLeaf1Twitch, value2STWindLeaf1Twitch);
			// STWindLeaf1Ripple (TIME, AMOUNT, 0, 0)
			material.SetVector (propSTWindLeaf1Ripple, value2STWindLeaf1Ripple);
			
			// STWindLeaf2Tumble (TIME, FLIP, TWIST, ADHERENCE)
			material.SetVector (propSTWindLeaf2Tumble, value2STWindLeaf2Tumble);
			// STWindLeaf2Twitch (AMOUNT, SHARPNESS, TIME, 0.0)
			material.SetVector (propSTWindLeaf2Twitch, value2STWindLeaf2Twitch);
			// STWindLeaf2Ripple (TIME, AMOUNT, 0, 0)
			material.SetVector (propSTWindLeaf2Ripple, value2STWindLeaf1Ripple);

		}
		/// <summary>
		/// Update params related to the first detected directional wind zone.
		/// </summary>
		/// <param name="treeController">Tree controller.</param>
		public void GetWindZoneValues () {
			bool isST8 = true;
			valueWindDirection = new Vector4 (1f, 0f, 0f, 0f);
			WindZone[] windZones = FindObjectsOfType<WindZone> ();
			for (int i = 0; i < windZones.Length; i++) {
				if (windZones [i].gameObject.activeSelf && windZones[i].mode == WindZoneMode.Directional) {
					valueWindMain = windZones [i].windMain;
					valueWindTurbulence = windZones [i].windTurbulence;
					valueWindDirection = new Vector4 (windZones [i].transform.forward.x, windZones [i].transform.forward.y, windZones [i].transform.forward.z, 1f);
					valueLeafSwayFactor = (isST8?0.4f:1f) * windZones [i].windMain;
					valueLeafTurbulenceFactor = (isST8?0.4f:1f) * windZones [i].windTurbulence;
					break;
				}
			}
		}
		#endregion
	}
}