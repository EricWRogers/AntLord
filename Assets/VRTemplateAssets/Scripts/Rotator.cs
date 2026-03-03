using UnityEngine;

namespace Unity.VRTemplate
{
    /// <summary>
    /// Rotates this object at a user defined speed
    /// </summary>
    
    public class Rotator : MonoBehaviour
    {
        [SerializeField, Tooltip("Angular velocity in degrees per second")]
        Vector3 m_Velocity;
        bool isRotating = false;

        void Update()
        {
            transform.Rotate(m_Velocity * Time.deltaTime);
        }

        public void ChangeRotation()
        {
            isRotating = !isRotating;

            if (isRotating)
                m_Velocity = new Vector3(10, 0, 10);
            
            if(!isRotating)
                m_Velocity = new Vector3(0, 0, 0);
        }
    }
}
