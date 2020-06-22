using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Clayxels;

namespace Clayxels{
	[ExecuteInEditMode]
	public class ClayObject : MonoBehaviour{
		public enum ClayObjectMode{
			single,
			offset,
			spline
		}

		public float blend = 0.0f;
		public Color color;
		public Vector4 attrs = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
		public int primitiveType = 0;
		public ClayObjectMode mode = ClayObjectMode.single;
		public GameObject offsetter = null;
		public List<GameObject> splinePoints = new List<GameObject>();
		public int splineSubdiv = 3;

		public int clayObjectId = -1;

		public WeakReference clayxelContainerRef = null;

		[SerializeField] int numSolids = 1;
		List<Solid> solids = new List<Solid>();
		bool invalidated = false;
		Color gizmoColor = new Color(1.0f, 1.0f, 1.0f, 0.5f);

		void Awake(){
			this.getClayContainer().scheduleClayObjectsScan();
		}

		void Update(){
			this.updateSolids(true);
		}

		public void forceUpdate(){
			this.transform.hasChanged = true;

			this.updateSolids(true);
		}

		public void pullUpdate(){
			this.updateSolids(false);
		}

		void updateSolids(bool notifyContainer){
			if(this.mode == ClayObjectMode.single){
				if(this.transform.hasChanged){
					this.transform.hasChanged = false;

					this.updateSingle();

					if(notifyContainer){
						this.getClayContainer().clayObjectUpdated(this);
					}
				}
			}
			else if(this.mode == ClayObjectMode.offset){
				if(this.transform.hasChanged || this.offsetter.transform.hasChanged){
					this.transform.hasChanged = false;
					this.offsetter.transform.hasChanged = false;

					this.updateOffset();

					if(notifyContainer){
						this.getClayContainer().clayObjectUpdated(this);
					}
				}
			}
			else if(this.mode == ClayObjectMode.spline){
				bool changed = false;

				if(this.transform.hasChanged){
					changed = true;
				}
				else{
					for(int i = 0; i < this.splinePoints.Count; ++i){
						if(this.splinePoints[i].transform.hasChanged){
							changed = true;
							break;
						}
					}
				}

				if(changed){
					this.transform.hasChanged = false;

					this.updateSpline();

					if(notifyContainer){
						this.getClayContainer().clayObjectUpdated(this);
					}
				}
			}
		}
		
		void OnDestroy(){
			this.invalidated = true;
			
			ClayContainer clayxel = this.getClayContainer();
			if(clayxel != null){
				clayxel.scheduleClayObjectsScan();
				
				#if UNITY_EDITOR
					if(!Application.isPlaying){
						UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
					}
				#endif
			}
		}

		public Solid getSolid(int id){
			return this.solids[id];
		}

		public int getNumSolids(){
			return this.solids.Count;
		}

		public void setOffsetNum(int num){
			this.numSolids = num;
			this.init();
			this.forceUpdate();

			this.getClayContainer().scheduleClayObjectsScan();
		}

		public void updateSplineSetup(){
			if(this.splineSubdiv < 0){
				this.splineSubdiv = 0;
			}

			if(this.splineSubdiv > 20){
				this.splineSubdiv = 20;
			}

			this.numSolids = this.splineSubdiv * (this.splinePoints.Count - 3);
			if(this.numSolids < 0){
				this.numSolids = 0;
			}

			this.init();
			this.forceUpdate();

			this.getClayContainer().scheduleClayObjectsScan();
		}

		public void init(){
			this.solids.Clear();
			this.changeNumSolids(this.numSolids);
			
			this.clayxelContainerRef = null;
			GameObject parent = this.transform.parent.gameObject;

			ClayContainer clayxel = null;
			for(int i = 0; i < 100; ++i){
				clayxel = parent.GetComponent<ClayContainer>();
				if(clayxel != null){
					break;
				}
				else{
					parent = parent.transform.parent.gameObject;
				}
			}

			if(clayxel == null){
				Debug.Log("failed to find parent clayxel container");
			}
			else{
				this.clayxelContainerRef = new WeakReference(clayxel);
			}
		}

		void updateSingle(){
			Solid solid = this.solids[0];

			Matrix4x4 invMat = this.getClayContainer().transform.worldToLocalMatrix * this.transform.localToWorldMatrix;

			solid.position.x = invMat[0, 3];
			solid.position.y = invMat[1, 3];
			solid.position.z = invMat[2, 3];

			solid.rotation = invMat.rotation;
			solid.scale = this.transform.localScale * 0.5f;

			solid.blend = this.blend;
			
			solid.color.x = this.color.r;
			solid.color.y = this.color.g;
			solid.color.z = this.color.b;

			solid.attrs = this.attrs;

			solid.primitiveType = this.primitiveType;
		}

		void updateOffset(){
			Matrix4x4 invMat = this.getClayContainer().transform.worldToLocalMatrix * this.transform.localToWorldMatrix;

			Vector3 offsetPos = new Vector3(invMat[0, 3], invMat[1, 3], invMat[2, 3]);
			Quaternion offsetRot = invMat.rotation;
			Vector3 offsetScale = this.transform.localScale;

			for(int i = 0; i < this.solids.Count; ++i){
				Solid solid = this.solids[i];

				solid.position = offsetPos;

				offsetPos += offsetRot * this.offsetter.transform.localPosition;

				solid.rotation = offsetRot;

				offsetRot = offsetRot * this.offsetter.transform.localRotation;

				solid.scale = offsetScale * 0.5f;

				offsetScale.x = offsetScale.x * this.offsetter.transform.localScale.x;
				offsetScale.y = offsetScale.y * this.offsetter.transform.localScale.y;
				offsetScale.z = offsetScale.z * this.offsetter.transform.localScale.z;

				solid.blend = this.blend;
				
				solid.color.x = this.color.r;
				solid.color.y = this.color.g;
				solid.color.z = this.color.b;

				solid.attrs = this.attrs;

				solid.primitiveType = this.primitiveType;
			}
		}

		void updateSpline(){
			if(this.splinePoints.Count > 3){
				float incrT = 1.0f / this.splineSubdiv;

				int solidIt = 0;

				Matrix4x4 parentInvMat = this.getClayContainer().transform.worldToLocalMatrix;
				
				for(int i = 0; i < this.splinePoints.Count - 3; ++i){
					GameObject splinePoint0 = this.splinePoints[i];
					GameObject splinePoint1 = this.splinePoints[i + 1];
					GameObject splinePoint2 = this.splinePoints[i + 2];
					GameObject splinePoint3 = this.splinePoints[i + 3];

					splinePoint0.transform.hasChanged = false;
					splinePoint1.transform.hasChanged = false;
					splinePoint2.transform.hasChanged = false;
					splinePoint3.transform.hasChanged = false;

					Vector3 s0;
					s0.x = this.transform.localScale.x * splinePoint0.transform.localScale.x;
					s0.y = this.transform.localScale.y * splinePoint0.transform.localScale.y;
					s0.z = this.transform.localScale.z * splinePoint0.transform.localScale.z;

					Vector3 s1 = splinePoint1.transform.localScale;
					s1.x = this.transform.localScale.x * splinePoint1.transform.localScale.x;
					s1.y = this.transform.localScale.y * splinePoint1.transform.localScale.y;
					s1.z = this.transform.localScale.z * splinePoint1.transform.localScale.z;

					Vector3 s2 = splinePoint2.transform.localScale;
					s2.x = this.transform.localScale.x * splinePoint2.transform.localScale.x;
					s2.y = this.transform.localScale.y * splinePoint2.transform.localScale.y;
					s2.z = this.transform.localScale.z * splinePoint2.transform.localScale.z;

					Vector3 s3 = splinePoint3.transform.localScale;
					s3.x = this.transform.localScale.x * splinePoint3.transform.localScale.x;
					s3.y = this.transform.localScale.y * splinePoint3.transform.localScale.y;
					s3.z = this.transform.localScale.z * splinePoint3.transform.localScale.z;

					Matrix4x4 pointMat0 = parentInvMat * splinePoint0.transform.localToWorldMatrix;
					Matrix4x4 pointMat1 = parentInvMat * splinePoint1.transform.localToWorldMatrix;
					Matrix4x4 pointMat2 = parentInvMat * splinePoint2.transform.localToWorldMatrix;
					Matrix4x4 pointMat3 = parentInvMat * splinePoint3.transform.localToWorldMatrix;

					Vector3 point0 = new Vector3(pointMat0[0, 3], pointMat0[1, 3], pointMat0[2, 3]);
					Vector3 point1 = new Vector3(pointMat1[0, 3], pointMat1[1, 3], pointMat1[2, 3]);
					Vector3 point2 = new Vector3(pointMat2[0, 3], pointMat2[1, 3], pointMat2[2, 3]);
					Vector3 point3 = new Vector3(pointMat3[0, 3], pointMat3[1, 3], pointMat3[2, 3]);

					for(int j = 0; j < this.splineSubdiv; ++j){
						float t = incrT * j;

						Solid solid = this.solids[solidIt];
						solid.position = this.getCatmullRomVec3(point0, point1, point2, point3, t);
						solid.rotation = this.getCatmullRomQuat(pointMat0.rotation, pointMat1.rotation, pointMat2.rotation, pointMat3.rotation, t);
						solid.scale = this.getCatmullRomVec3(s0, s1, s2, s3, t) * 0.5f;

						solid.blend = this.blend;
						solid.attrs = this.attrs;
						solid.primitiveType = this.primitiveType;

						solid.color.x = this.color.r;
						solid.color.y = this.color.g;
						solid.color.z = this.color.b;

						solidIt += 1;
					}
				}
			}
		}

		public bool isValid(){
			return !this.invalidated;
		}

		void changeNumSolids(int num){
			this.solids = new List<Solid>(num);

			for(int i = 0; i < num; ++i){
				this.solids.Add(new Solid());
			}
		}

		public void setMode(ClayObjectMode mode){
			if(mode == this.mode){
				return;
			}

			this.mode = mode;

			if(this.mode == ClayObjectMode.offset){
				if(this.offsetter == null){
					GameObject offsetObj = new GameObject("offsetter");
					offsetObj.transform.parent = this.transform;
					offsetObj.transform.localPosition = new Vector3(0.0f, 0.5f, 0.0f);
					offsetObj.transform.localEulerAngles = new Vector3(0.0f, 30.0f, 0.0f);
					offsetObj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
					this.numSolids = 3;
					this.offsetter = offsetObj;
				}
			}
			else if(this.mode == ClayObjectMode.spline){
				if(this.splinePoints.Count == 0){
					GameObject offsetObj = new GameObject("splinePnt1");
					offsetObj.transform.parent = this.transform;
					offsetObj.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
					offsetObj.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
					offsetObj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
					this.splinePoints.Add(offsetObj);
					this.splinePoints.Add(offsetObj);

					offsetObj = new GameObject("splinePnt2");
					offsetObj.transform.parent = this.transform;
					offsetObj.transform.localPosition = new Vector3(2.0f, 1.0f, 0.0f);
					offsetObj.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
					offsetObj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
					this.splinePoints.Add(offsetObj);

					offsetObj = new GameObject("splinePnt3");
					offsetObj.transform.parent = this.transform;
					offsetObj.transform.localPosition = new Vector3(3.0f, 0.0f, 0.0f);
					offsetObj.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
					offsetObj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
					this.splinePoints.Add(offsetObj);

					offsetObj = new GameObject("splinePnt4");
					offsetObj.transform.parent = this.transform;
					offsetObj.transform.localPosition = new Vector3(4.0f, 0.0f, 0.0f);
					offsetObj.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
					offsetObj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
					this.splinePoints.Add(offsetObj);
					this.splinePoints.Add(offsetObj);

					this.updateSplineSetup();
				}
			}

			this.init();

			this.forceUpdate();

			this.getClayContainer().scheduleClayObjectsScan();
		}

		public ClayContainer getClayContainer(){
			if(this.clayxelContainerRef == null){
				this.init();
			}

			return (ClayContainer)this.clayxelContainerRef.Target;
		}

		public void setClayxelContainer(ClayContainer container){
			this.clayxelContainerRef = new WeakReference(container);
		}

		public void setPrimitiveType(int primType){
			this.primitiveType = primType;
		}

		public Color getColor(){
			return this.color;
		}

		Vector3 getCatmullRomVec3(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t){
			//The coefficients of the cubic polynomial (except the 0.5f * which I added later for performance)
			Vector3 a = 2f * p1;
			Vector3 b = p2 - p0;
			Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
			Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

			//The cubic polynomial: a + b * t + c * t^2 + d * t^3
			Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

			return pos;
		}

		Quaternion getCatmullRomQuat(Quaternion p0, Quaternion p1, Quaternion p2, Quaternion p3, float t){
			Quaternion a;
			a.x = p1.x * 2.0f;
			a.y = p1.y * 2.0f;
			a.z = p1.z * 2.0f;
			a.w = p1.w * 2.0f;

			Quaternion b;
			b.x = p2.x - p0.x;
			b.y = p2.y - p0.y;
			b.z = p2.z - p0.z;
			b.w = p2.w - p0.w;

			Quaternion c;
			c.x = 2.0f * p0.x - 5.0f * p1.x + 4.0f * p2.x - p3.x;
			c.y = 2.0f * p0.y - 5.0f * p1.y + 4.0f * p2.y - p3.y;
			c.z = 2.0f * p0.z - 5.0f * p1.z + 4.0f * p2.z - p3.z;
			c.w = 2.0f * p0.w - 5.0f * p1.w + 4.0f * p2.w - p3.w;

			Quaternion d;
			d.x = -p0.x + 3.0f * p1.x - 3.0f * p2.x + p3.x;
			d.y = -p0.y + 3.0f * p1.y - 3.0f * p2.y + p3.y;
			d.z = -p0.z + 3.0f * p1.z - 3.0f * p2.z + p3.z;
			d.w = -p0.w + 3.0f * p1.w - 3.0f * p2.w + p3.w;

			Quaternion rot;
			float pow2 = t * t;
			float pow3 = t * t * t;
			rot.x = 0.5f * (a.x + (b.x * t) + (c.x * pow2) + (d.x * pow3));
			rot.y = 0.5f * (a.y + (b.y * t) + (c.y * pow2) + (d.y * pow3));
			rot.z = 0.5f * (a.z + (b.z * t) + (c.z * pow2) + (d.z * pow3));
			rot.w = 0.5f * (a.w + (b.w * t) + (c.w * pow2) + (d.w * pow3));

			return rot.normalized;
		}

		#if UNITY_EDITOR
		public virtual void onInspectorGUI(){
			Debug.Log("insp clayObj");
		}

		void OnDrawGizmos(){
			if(this.blend < 0.0f || // negative shape?
				(((int)this.attrs.w >> 0)&1) == 1){// painter?

				if(UnityEditor.Selection.Contains(this.gameObject)){// if selected draw wire cage
					Gizmos.color = this.gizmoColor;
					if(this.primitiveType == 0){
						Gizmos.matrix = this.transform.localToWorldMatrix;
						Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
					}
					else if(this.primitiveType == 1){
						Gizmos.matrix = this.transform.localToWorldMatrix;
						Gizmos.DrawWireSphere(Vector3.zero, 0.5f);
					}
					else if(this.primitiveType == 2){
						this.drawCylinder();
					}
					else if(this.primitiveType == 3){
						this.drawTorus();
					}
					else if(this.primitiveType == 4){
						this.drawCurve();
					}
					else if(this.primitiveType == 5){
						Gizmos.matrix = this.transform.localToWorldMatrix;
						Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
					}
					else if(this.primitiveType == 6){
						Gizmos.matrix = this.transform.localToWorldMatrix;
						Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
					}
				}
			}
		}

		void drawCurve(){
			Handles.color = Color.white;
			
			float radius = this.attrs.z * 0.5f;
			Vector3 heightVec = (this.transform.up * (this.transform.localScale.y - this.attrs.z)) * 0.5f;
			Vector3 sideVec = this.transform.right * ((this.transform.localScale.x*0.5f) - radius);
			Vector3 startPnt = this.transform.position - sideVec - heightVec;
			Vector3 endPnt = this.transform.position + sideVec - heightVec;
			Vector3 tanOffset = this.transform.right * - (this.transform.localScale.x * 0.2f);
			Vector3 tanOffset2 = this.transform.up * (radius * 0.5f);
			Vector3 tanSlide = this.transform.right * ((this.attrs.x - 0.5f) * (this.transform.localScale.x * 0.5f));
			Vector3 startTan = this.transform.position + heightVec + tanOffset + tanOffset2 + tanSlide;
			Vector3 endTan = this.transform.position + heightVec - tanOffset + tanOffset2 + tanSlide;
			Vector3 elongVec =  this.transform.forward * ((this.transform.localScale.z * 0.5f) - radius);
			Vector3 elongVec2 =  this.transform.forward * ((this.transform.localScale.z * 0.5f) - (radius*2.0f));

			float w0 = (1.0f - this.attrs.y) * 2.0f;
			float w1 = this.attrs.y * 2.0f;

			Handles.DrawBezier(startPnt - (elongVec*w0), endPnt - (elongVec*w1), startTan - elongVec, endTan - elongVec, Color.white, null, 2.0f);
			Handles.DrawBezier(startPnt + (elongVec*w0), endPnt + (elongVec*w1), startTan + elongVec, endTan + elongVec, Color.white, null, 2.0f);

			Gizmos.DrawWireSphere(startPnt - elongVec2, radius * w0);
			Gizmos.DrawWireSphere(endPnt - elongVec2, radius * w1);

			if(this.transform.localScale.z > 1.0f){
				Gizmos.DrawWireSphere(startPnt + elongVec2, radius);
				Gizmos.DrawWireSphere(endPnt + elongVec2, radius);

				Handles.DrawLine(
					startPnt + elongVec2 - (this.transform.right * radius), 
					startPnt - elongVec2 - (this.transform.right * radius));

				Handles.DrawLine(
					endPnt + elongVec2 + (this.transform.right * radius), 
					endPnt - elongVec2 + (this.transform.right * radius));
			}
		}

		void drawTorus(){
			Handles.color = Color.white;

			float radius = this.attrs.x;

			Vector3 elongationVec = this.transform.forward * ((this.transform.localScale.z * 0.5f) - radius);
			Vector3 sideVec = this.transform.right * ((this.transform.localScale.x * 0.5f) - radius);
			Vector3 radiusSideOffsetVec = this.transform.right * radius;
			Vector3 heightVec = this.transform.up * ((this.transform.localScale.y * 0.5f) - radius);
			Vector3 radiusUpOffsetVec = this.transform.up * radius;
			Vector3 sideCrossSecVec = this.transform.right * (this.transform.localScale.x * 0.5f);

			float crossSecRadius = this.transform.localScale.x * 0.5f;
			Vector3 radiusCrossSecVec = this.transform.up * crossSecRadius;
			Vector3 heightCrossSecVec = this.transform.up * ((this.transform.localScale.y *0.5f) - crossSecRadius);

			float crossSecRadiusIn = (this.transform.localScale.x * 0.5f) - (radius*2.0f);
			Vector3 sideCrossSecVecIn = this.transform.right * ((this.transform.localScale.x * 0.5f) - (radius * 2.0f));

			if(this.transform.localScale.y >= this.transform.localScale.x){
				// cross out section
				Handles.DrawWireArc(this.transform.position + heightCrossSecVec, 
					this.transform.forward, this.transform.right, 180.0f, crossSecRadius);

				Handles.DrawWireArc(this.transform.position - heightCrossSecVec, 
					this.transform.forward, this.transform.right, -180.0f, crossSecRadius);

				Handles.DrawLine(
					this.transform.position + heightCrossSecVec + sideCrossSecVec, 
					this.transform.position - heightCrossSecVec + sideCrossSecVec);

				Handles.DrawLine(
					this.transform.position + heightCrossSecVec - sideCrossSecVec, 
					this.transform.position - heightCrossSecVec - sideCrossSecVec);

				// cross in section
				Handles.DrawWireArc(this.transform.position + heightCrossSecVec, 
					this.transform.forward, this.transform.right, 180.0f, crossSecRadiusIn);

				Handles.DrawWireArc(this.transform.position - heightCrossSecVec, 
					this.transform.forward, this.transform.right, -180.0f, crossSecRadiusIn);

				Handles.DrawLine(
					this.transform.position + heightCrossSecVec + sideCrossSecVecIn, 
					this.transform.position - heightCrossSecVec + sideCrossSecVecIn);

				Handles.DrawLine(
					this.transform.position + heightCrossSecVec - sideCrossSecVecIn, 
					this.transform.position - heightCrossSecVec - sideCrossSecVecIn);
			}

			if(this.transform.localScale.z >= radius * 2.0f){
				// top section
				Handles.DrawWireArc(this.transform.position - elongationVec + heightVec, 
					this.transform.right, this.transform.up, -180.0f, radius);

				Handles.DrawWireArc(this.transform.position + elongationVec + heightVec, 
					this.transform.right, this.transform.up, 180.0f, radius);

				Handles.DrawLine(
					this.transform.position + elongationVec + heightVec + radiusUpOffsetVec , 
					this.transform.position - elongationVec + heightVec + radiusUpOffsetVec);

				Handles.DrawLine(
					this.transform.position + elongationVec + heightVec - radiusUpOffsetVec , 
					this.transform.position - elongationVec + heightVec - radiusUpOffsetVec);

				// bottom section
				Handles.DrawWireArc(this.transform.position - elongationVec - heightVec, 
					this.transform.right, this.transform.up, -180.0f, radius);

				Handles.DrawWireArc(this.transform.position + elongationVec - heightVec, 
					this.transform.right, this.transform.up, 180.0f, radius);

				Handles.DrawLine(
					this.transform.position + elongationVec - heightVec + radiusUpOffsetVec , 
					this.transform.position - elongationVec - heightVec + radiusUpOffsetVec);

				Handles.DrawLine(
					this.transform.position + elongationVec - heightVec - radiusUpOffsetVec , 
					this.transform.position - elongationVec - heightVec - radiusUpOffsetVec);

				// left section
				Handles.DrawWireArc(this.transform.position - elongationVec - sideVec, 
					this.transform.up, this.transform.right, 180.0f, radius);

				Handles.DrawWireArc(this.transform.position + elongationVec - sideVec, 
					this.transform.up, this.transform.right, -180.0f, radius);

				Handles.DrawLine(
					this.transform.position + elongationVec - sideVec + radiusSideOffsetVec , 
					this.transform.position - elongationVec - sideVec + radiusSideOffsetVec);

				Handles.DrawLine(
					this.transform.position + elongationVec - sideVec - radiusSideOffsetVec, 
					this.transform.position - elongationVec - sideVec - radiusSideOffsetVec);

				// right section
				Handles.DrawWireArc(this.transform.position - elongationVec + sideVec, 
					this.transform.up, this.transform.right, 180.0f, radius);

				Handles.DrawWireArc(this.transform.position + elongationVec + sideVec, 
					this.transform.up, this.transform.right, -180.0f, radius);

				Handles.DrawLine(
					this.transform.position + elongationVec + sideVec + radiusSideOffsetVec , 
					this.transform.position - elongationVec + sideVec + radiusSideOffsetVec);

				Handles.DrawLine(
					this.transform.position + elongationVec + sideVec - radiusSideOffsetVec, 
					this.transform.position - elongationVec + sideVec - radiusSideOffsetVec);
			}
		}

		void drawCylinder(){
			Handles.color = Color.white;
			
			float radius = this.transform.localScale.x;
			if(this.transform.localScale.z < radius){
				radius = this.transform.localScale.z;
			}

			radius *= 0.5f;

			Vector3 arcDir = this.transform.right;
			Vector3 extVec = - (this.transform.forward * ((this.transform.localScale.z * 0.5f) - radius));
			if(this.transform.localScale.z < this.transform.localScale.x){
				arcDir = this.transform.forward;
				extVec = (this.transform.right * ((this.transform.localScale.x*0.5f) - radius));
			}

			Vector3 heightVec = this.transform.up * (this.transform.localScale.y * 0.5f);

			// draw top
			Handles.DrawWireArc(this.transform.position + extVec + heightVec, this.transform.up, arcDir, 180.0f, radius);
			Handles.DrawWireArc(this.transform.position - extVec + heightVec, this.transform.up, arcDir, -180.0f, radius);

			Handles.DrawLine(
				this.transform.position + extVec + heightVec + (arcDir*radius), 
				this.transform.position - extVec + heightVec + (arcDir*radius));

			Handles.DrawLine(
				this.transform.position + extVec + heightVec - (arcDir*radius), 
				this.transform.position - extVec + heightVec - (arcDir*radius));

			// draw bottom
			Handles.DrawWireArc(this.transform.position + extVec - heightVec, this.transform.up, arcDir, 180.0f, radius+this.attrs.z);
			Handles.DrawWireArc(this.transform.position - extVec - heightVec, this.transform.up, arcDir, -180.0f, radius+this.attrs.z);
			
			Handles.DrawLine(
				this.transform.position + extVec - heightVec - (arcDir*(radius+this.attrs.z)), 
				this.transform.position - extVec - heightVec - (arcDir*(radius+this.attrs.z)));

			Handles.DrawLine(
				this.transform.position + extVec - heightVec + (arcDir*(radius+this.attrs.z)), 
				this.transform.position - extVec - heightVec + (arcDir*(radius+this.attrs.z)));

			// draw side lines
			Handles.DrawLine(
				this.transform.position + heightVec + (arcDir*radius), 
				this.transform.position - heightVec + (arcDir*(radius+this.attrs.z)));

			Handles.DrawLine(
				this.transform.position + heightVec - (arcDir*radius), 
				this.transform.position - heightVec - (arcDir*(radius+this.attrs.z)));
		}
		#endif // end if UNITY_EDITOR 
	}
}
