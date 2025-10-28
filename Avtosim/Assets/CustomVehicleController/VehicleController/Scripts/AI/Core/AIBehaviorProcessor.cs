using System.Collections.Generic;
using UnityEngine;

namespace Assets.VehicleController
{
    public class AIBehaviorProcessor
    {
        private int _arraySize;
        private int _behaviorCount;

        private int _rightSideEndIndex;
        private int _leftSideBeginIndex;

        private int _highestInterestID;
        public int HighestInterestID => _highestInterestID;

        private float[] _dangerArray;
        private float[] _interestArray;

        private float _lowestDanger = 1;
        public float LowestDanger => _lowestDanger;
        private float _leftDangerSum = 0;
        private float _rightDangerSum = 0;

        private List<AIBehavior> _behaviorList;
        private float[] _dangerDotArray;

        public AIBehaviorProcessor(int arraySize)
        {
            _arraySize = arraySize;
            _dangerArray = new float[_arraySize];
            _interestArray = new float[_arraySize];
            _dangerDotArray = new float[_arraySize];

            _behaviorList = new List<AIBehavior>()
            {
                new AIEvadeStaticObjectsBehavior(_dangerArray, _interestArray, _dangerDotArray),
                new AIEvadeDynamicObjectsBehavior(_dangerArray, _interestArray, _dangerDotArray),
            };
            _behaviorCount = _behaviorList.Count;
            _rightSideEndIndex = _arraySize / 2;
            _leftSideBeginIndex = _rightSideEndIndex + 1;
        }

        public Vector3 GetDirectionalVector(RaycastHitInfo[] hitInfo, Vector3 trackForwardDirection, int interestModifier, int dangerModifier)
        {
            ResetDangerArray();
            CalculateDotDirectionArray(hitInfo, trackForwardDirection, dangerModifier);
            CalculateInterest(interestModifier);
            ProcessBehaviors(hitInfo, trackForwardDirection);
            FindDangerData();
            return MakeDecision(hitInfo);
        }

        private void ProcessBehaviors(RaycastHitInfo[] hitInfo, Vector3 trackForwardDirection)
        {
            for(int i = 0; i < _behaviorCount; i++)
                _behaviorList[i].ProcessData(hitInfo, trackForwardDirection);
        }

        private void FindDangerData()
        {
            _lowestDanger = 1;
            _leftDangerSum = 0;
            _rightDangerSum = 0;


            for (int i = 0; i < _rightSideEndIndex; i++)
            {
                float danger = _dangerArray[i];
                if (danger <  _lowestDanger)
                    _lowestDanger = danger;

                _rightDangerSum += danger;
            }

            for(int i = _leftSideBeginIndex; i < _arraySize; i++) 
            {
                float danger = _dangerArray[i];
                if (danger < _lowestDanger)
                    _lowestDanger = danger;

                _leftDangerSum += danger;
            }
        }

        private Vector3 MakeDecision(RaycastHitInfo[] hitInfo)
        {
            float highestInterest = 0;
            _highestInterestID = 0;

            float rightSideInterestMultiplier = 1;
            float leftSideInterestMultiplier = 1;

            if (_rightDangerSum != _leftDangerSum)
            {
                leftSideInterestMultiplier = 1 +  _rightDangerSum / (_leftDangerSum + 1);
                rightSideInterestMultiplier = 1 + _leftDangerSum / (_rightDangerSum + 1);
            }

            for (int i = 0; i < _rightSideEndIndex; i++)
            {
                if (_dangerArray[i] > _lowestDanger)
                    continue;

                float interest = (_interestArray[i] - _dangerArray[i]) * rightSideInterestMultiplier;
                if (interest > highestInterest)
                {
                    highestInterest = interest;
                    _highestInterestID = i;
                }
            }
            

            for (int i = _leftSideBeginIndex; i < _arraySize; i++)
            {
                if (_dangerArray[i] > _lowestDanger)
                    continue;

                float interest = (_interestArray[i] - _dangerArray[i]) * leftSideInterestMultiplier;
                if (interest > highestInterest)
                {
                    highestInterest = interest;
                    _highestInterestID = i;
                }
            }

            return hitInfo[_highestInterestID].Direction;
        }

        private void CalculateInterest(int interestMod)
        {
            for(int i = 0; i < _arraySize; i++)
                _interestArray[i] = Mathf.Pow(_dangerDotArray[i], interestMod);
        }

        private void CalculateDotDirectionArray(RaycastHitInfo[] hitInfo, Vector3 trackForwardDirection, int dangerMod)
        {
            for(int i = 0; i < _arraySize; i++)
            {
                float dot = Vector3.Dot(hitInfo[i].Direction, trackForwardDirection);
                _dangerDotArray[i] = Mathf.Pow(dot, dangerMod);
            }
        }
        public float[] GetDangerArray() => _dangerArray;
        private void ResetDangerArray()
        {
            for (int i = 0; i < _arraySize; i++)
                _dangerArray[i] = 0; 
        }
    }
}
