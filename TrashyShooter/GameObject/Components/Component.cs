namespace MultiplayerEngine
{
    /// <summary>
    /// the core for the components
    /// Niels
    /// </summary>
    public abstract class Component
    {
        #region Fields & Properties
        /// <summary>
        /// the transform belonging to the gameobject that has this component
        /// </summary>
        public Transform transform;
        /// <summary>
        /// the gameobject this component is on
        /// </summary>
        public GameObject gameObject;
        /// <summary>
        /// if false all methods in the Component won't be called
        /// </summary>
        public bool enabled = true;
        #endregion
    }
}