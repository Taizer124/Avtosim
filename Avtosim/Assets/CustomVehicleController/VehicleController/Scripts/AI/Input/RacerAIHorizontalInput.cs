using UnityEngine;

namespace Assets.VehicleController
{
    public class RacerAIHorizontalInput
    {
        private float _selectedSteerDirectionToReverse = 0;
        public float GetHorizontalInput(Vector3 localRight, Vector3 heading, float dotToTrack, AIRacerState state)
        {
            if (state == AIRacerState.Reversing)
            {
                _selectedSteerDirectionToReverse = 0;
                return -CalculateTurnAmount(localRight, heading, dotToTrack);
            }

            if (state == AIRacerState.FollowingTrack)
            {
                _selectedSteerDirectionToReverse = 0;
                return CalculateTurnAmount(localRight, heading, dotToTrack);
            }

            if (_selectedSteerDirectionToReverse == 0)
            {
                float turnAmount = CalculateTurnAmount(localRight, heading, dotToTrack);
                if (turnAmount < 0)
                    _selectedSteerDirectionToReverse = - 1;
                else
                    _selectedSteerDirectionToReverse = 1;
            }


            return _selectedSteerDirectionToReverse;
        }

        private float CalculateTurnAmount(Vector3 localRight, Vector3 headingDirection, float dotToTrack)
        {
            float turnAmount = Vector3.Dot(localRight, headingDirection);

            if(dotToTrack < 0.33f)
            {
                if (turnAmount < 0)
                    return -1;
                return 1;
            }

            return turnAmount;
        }
    }
}
