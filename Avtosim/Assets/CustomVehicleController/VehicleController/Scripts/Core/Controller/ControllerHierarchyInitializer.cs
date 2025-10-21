using UnityEditor;
using UnityEngine;

namespace Assets.VehicleController
{
#if UNITY_EDITOR
    [RequireComponent(typeof(CustomVehicleController))]
    public class ControllerHierarchyInitializer
    {
        private Transform[] _wheelTransforms;
        private Transform[] _brakesTransforms;
        private Transform[] _steerWheelTransforms;

        private DrivetrainType _drivetrainType;
        private Transform _centerOfMass;

        private Transform[] _steerParentTransforms;
        private WheelController[] _wheelControllers;
        private WheelController[] _steerWheelControllers;

        private VehicleAxle[] _vehicleAxleArray;

        private Rigidbody _rigidbody;

        public void CreateHierarchyAndInitializeController(SerializedObject serializedController,
            SerializedObject serializedCarVisuals, CustomVehicleController controller, MeshRenderer mesh)
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Create Controller Hierarchy");
            int group = Undo.GetCurrentGroup();

            GameObject wheelsParent = new("Wheels");
            Undo.RegisterCreatedObjectUndo(wheelsParent, "Wheels Parent GO");
            wheelsParent.transform.parent = controller.transform.root;
            wheelsParent.transform.localPosition = new(0, 0, 0);
            wheelsParent.transform.localRotation = Quaternion.identity;

            GameObject wheelsMeshesParent = new("WheelsMeshes");
            Undo.RegisterCreatedObjectUndo(wheelsMeshesParent, "Wheel Meshes Parent GO");
            wheelsMeshesParent.transform.parent = wheelsParent.transform;
            wheelsMeshesParent.transform.localPosition = new(0, 0, 0);
            wheelsMeshesParent.transform.localRotation = Quaternion.identity;

            GameObject wheelControllersParent = new("WheelControllers");
            Undo.RegisterCreatedObjectUndo(wheelControllersParent, "Wheel Controllers Parent GO");
            wheelControllersParent.transform.parent = wheelsParent.transform;
            wheelControllersParent.transform.localPosition = new(0, 0, 0);
            wheelControllersParent.transform.localRotation = Quaternion.identity;

            _rigidbody = controller.GetComponent<Rigidbody>();

            CreateWheelsHierarcy(wheelsMeshesParent.transform, wheelControllersParent.transform);
            TryMoveUpControllers();
            CreateSteerWheelsHierarcy(wheelsMeshesParent.transform);

            CreateAxles();

            CreateCoM(controller.transform, mesh);
            InjectCustomVehicleFields(serializedController);
            InjectCarVisualsFields(serializedCarVisuals);
            Undo.CollapseUndoOperations(group);
        }

        private void CreateAxles()
        {
            _vehicleAxleArray = new VehicleAxle[_wheelTransforms.Length / 2];

            GameObject frontAxle = new GameObject("FrontAxle");
            Undo.RegisterCreatedObjectUndo(frontAxle, "Controller Front Axle Created");
            _vehicleAxleArray[0] = frontAxle.AddComponent<VehicleAxle>();

            Undo.SetTransformParent(frontAxle.transform, _wheelControllers[0].transform.parent, "Front Axle Set Parent");
            frontAxle.transform.localPosition = Vector3.zero;
            Undo.SetTransformParent(_wheelControllers[0].transform, frontAxle.transform, "Front Left Wheel Set Parent");

            _vehicleAxleArray[0].SetLeftHalfShaft(_wheelControllers[0], _wheelTransforms[0], _steerParentTransforms[0], _brakesTransforms[0]);

            Undo.SetTransformParent(_wheelControllers[1].transform, frontAxle.transform, "Front Right Wheel Set Parent");
            _vehicleAxleArray[0].SetRightHalfShaft(_wheelControllers[1], _wheelTransforms[1], _steerParentTransforms[1], _brakesTransforms[1]);

            GameObject rearAxle = new GameObject("RearAxle");
            Undo.RegisterCreatedObjectUndo(rearAxle, "Controller Rear Axle Created");
            _vehicleAxleArray[1] = rearAxle.AddComponent<VehicleAxle>();

            Undo.SetTransformParent(rearAxle.transform, _wheelControllers[2].transform.parent, "Rear Axle Set Parent");
            rearAxle.transform.localPosition = Vector3.zero;
            Undo.SetTransformParent(_wheelControllers[2].transform, rearAxle.transform, "Front Right Wheel Set Parent");
            _vehicleAxleArray[1].SetLeftHalfShaft(_wheelControllers[2], _wheelTransforms[2], null, _brakesTransforms[2]);

            Undo.SetTransformParent(_wheelControllers[3].transform, rearAxle.transform, "Front Right Wheel Set Parent");
            _vehicleAxleArray[1].SetRightHalfShaft(_wheelControllers[3], _wheelTransforms[3], null, _brakesTransforms[3]);
        }

        private void CreateWheelsHierarcy(Transform meshesParent, Transform controllerParent)
        {
            int size = _wheelTransforms.Length;
            _wheelControllers = new WheelController[size];

            for (int i = 0; i < size; i++)
            {
                Undo.SetTransformParent(_wheelTransforms[i], meshesParent, $"Wheel Transform Before Hierarchy Change {i}");

                if (_brakesTransforms[i] != null)
                    Undo.SetTransformParent(_brakesTransforms[i], meshesParent, $"Brake Transform Before Hierarchy Change {i}");

                GameObject wheelObj = new(_wheelTransforms[i].name + "_CONTROLLER");
                Undo.RegisterCreatedObjectUndo(wheelObj, "Controller Game Object");

                _wheelControllers[i] = wheelObj.AddComponent<WheelController>();
                _wheelControllers[i].SetWheelMeshTransform(_wheelTransforms[i]);
                TrySetWheelRadius(_wheelControllers[i]);

                wheelObj.transform.parent = controllerParent.transform;
                wheelObj.transform.localPosition = _wheelTransforms[i].transform.localPosition + new Vector3(0, _wheelControllers[i].WheelRadius * 0.125f, 0);
                wheelObj.transform.localRotation = Quaternion.identity;
            }
        }

        private void TrySetWheelRadius(WheelController wheelController)
        {
            if (wheelController.GetWheelTransform().TryGetComponent<MeshRenderer>(out MeshRenderer mesh))
            {
                SerializedObject so = new SerializedObject(wheelController);
                SerializedProperty wheelRadius = so.FindProperty("_wheelRadius");
                wheelRadius.floatValue = mesh.bounds.size.y / 2;
                so.ApplyModifiedProperties();
                so.Update();
            }
        }

        private void CreateSteerWheelsHierarcy(Transform meshesParent)
        {
            int size = _steerWheelTransforms.Length;
            _steerWheelControllers = new WheelController[size];
            _steerParentTransforms = new Transform[size];
            for (int i = 0; i < size; i++)
            {
                GameObject steerWheelParent = new($"SteerWheel {i}");
                Undo.RegisterCreatedObjectUndo(steerWheelParent, $"Steer Wheel Parent {i}");

                steerWheelParent.transform.parent = meshesParent.transform;
                steerWheelParent.transform.localPosition = _steerWheelTransforms[i].localPosition;
                steerWheelParent.transform.localRotation = Quaternion.identity;

                _steerWheelControllers[i] = _wheelControllers[i];

                Undo.SetTransformParent(_steerWheelTransforms[i], steerWheelParent.transform, $"{i} Steer Parenting");
                _steerWheelTransforms[i].transform.localPosition = new(0, 0, 0);

                _steerParentTransforms[i] = steerWheelParent.transform;
            }
        }

        private void TryMoveUpControllers()
        {
            int size = _wheelTransforms.Length;
            for (int i = 0; i < size; i++)
            {
                if (_wheelTransforms[i].TryGetComponent<MeshRenderer>(out MeshRenderer mesh))
                {
                    _wheelControllers[i].transform.position = new(_wheelControllers[i].transform.position.x,
                        _wheelControllers[i].transform.position.y + mesh.bounds.size.y / 2,
                        _wheelControllers[i].transform.position.z);
                }
                else
                {
                    Debug.LogWarning($"Wheel {_wheelTransforms[i].name} has no mesh renderer, " +
                        $"but you need to move the game object with wheel controller script up " +
                        $"to simulate suspension top point");
                }
            }
        }

        private void CreateCoM(Transform transform, MeshRenderer mesh)
        {
            if (mesh == null)
                Debug.LogWarning("Mesh Renderer wasn't provided, so Center Of Mass position couldn't be calculated automatically.");

            Vector3 position = Vector3.zero;

            //create temporary box collider to find the true center of body
            //if the origin of the body is not in the center, mesh.localBounds.center doesn't give correct results
            if (mesh != null)
            {
                BoxCollider tempBox = mesh.gameObject.AddComponent<BoxCollider>();
                position = transform.root.InverseTransformPoint(tempBox.bounds.center);
                GameObject.DestroyImmediate(tempBox);
            }

            GameObject _centerOfMassGO = new GameObject("CenterOfMass");
            _centerOfMass = _centerOfMassGO.transform;
            Undo.RegisterCreatedObjectUndo(_centerOfMassGO, "Center Of Mass Creation");
            Undo.SetTransformParent(_centerOfMass.transform, transform.root, "Center Of Mass Parenting");
            _centerOfMass.transform.localPosition = position;
            _centerOfMass.transform.localRotation = Quaternion.identity;
        }

        private void InjectCarVisualsFields(SerializedObject serializedCarVisuals)
        {
            serializedCarVisuals.Update();
            //set meshes
            var axleArray = serializedCarVisuals.FindProperty("_axleArray");
            axleArray.ClearArray();
            axleArray.InsertArrayElementAtIndex(0);
            axleArray.GetArrayElementAtIndex(0).objectReferenceValue = _vehicleAxleArray[0];
            axleArray.InsertArrayElementAtIndex(1);
            axleArray.GetArrayElementAtIndex(1).objectReferenceValue = _vehicleAxleArray[1];

            //set steer wheels parents
            var steerAxleArray = serializedCarVisuals.FindProperty("_steerAxleArray");
            steerAxleArray.ClearArray();
            steerAxleArray.InsertArrayElementAtIndex(0);
            steerAxleArray.GetArrayElementAtIndex(0).objectReferenceValue = _vehicleAxleArray[0];

            serializedCarVisuals.ApplyModifiedProperties();
            serializedCarVisuals.Update();
        }

        private void InjectCustomVehicleFields(SerializedObject serializedController)
        {
            serializedController.Update();

            //set meshes
            var frontAxleArray = serializedController.FindProperty("_frontAxles");
            frontAxleArray.ClearArray();
            frontAxleArray.InsertArrayElementAtIndex(0);
            frontAxleArray.GetArrayElementAtIndex(0).objectReferenceValue = _vehicleAxleArray[0];

            var rearAxleArray = serializedController.FindProperty("_rearAxles");
            rearAxleArray.ClearArray();
            rearAxleArray.InsertArrayElementAtIndex(0);
            rearAxleArray.GetArrayElementAtIndex(0).objectReferenceValue = _vehicleAxleArray[1];

            var steerAxleArray = serializedController.FindProperty("_steerAxles");
            steerAxleArray.ClearArray();
            steerAxleArray.InsertArrayElementAtIndex(0);
            steerAxleArray.GetArrayElementAtIndex(0).objectReferenceValue = _vehicleAxleArray[0];

            serializedController.FindProperty("_centerOfMass").objectReferenceValue = _centerOfMass;
            serializedController.FindProperty("DrivetrainType").intValue = (int)_drivetrainType;
            serializedController.FindProperty("_rigidbody").objectReferenceValue = _rigidbody;
            serializedController.ApplyModifiedProperties();
            serializedController.Update();
        }

        public void SetWheelTransforms(Transform[] wheelTransforms)
        {
            _wheelTransforms = wheelTransforms;
        }
        public void SetSteerWheelTransforms(Transform[] steerTransforms)
        {
            _steerWheelTransforms = steerTransforms;
        }

        public void SetBrakes(Transform[] brakes)
        {
            _brakesTransforms = brakes;
        }
    }
#endif
}

