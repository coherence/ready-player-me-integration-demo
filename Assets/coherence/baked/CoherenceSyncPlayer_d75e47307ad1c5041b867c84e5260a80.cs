// Copyright (c) coherence ApS.
// For all coherence generated code, the coherence SDK license terms apply. See the license file in the coherence Package root folder for more information.

// <auto-generated>
// Generated file. DO NOT EDIT!
// </auto-generated>
namespace Coherence.Generated
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Coherence.Toolkit;
    using Coherence.Toolkit.Bindings;
    using Coherence.Entities;
    using Coherence.ProtocolDef;
    using Coherence.Brook;
    using Coherence.Toolkit.Bindings.ValueBindings;
    using Coherence.Toolkit.Bindings.TransformBindings;
    using Coherence.Connection;
    using Coherence.Log;
    using Logger = Coherence.Log.Logger;
    using UnityEngine.Scripting;
    
    public class Binding_d75e47307ad1c5041b867c84e5260a80_4611e264_3ddd_458c_8f2a_444cd5794bad : PositionBinding
    {   
        private global::UnityEngine.Transform CastedUnityComponent;

        protected override void OnBindingCloned()
        {
    	    CastedUnityComponent = (global::UnityEngine.Transform)UnityComponent;
        }

        public override string CoherenceComponentName => "WorldPosition";
        public override uint FieldMask => 0b00000000000000000000000000000001;

        public override UnityEngine.Vector3 Value
        {
            get { return (UnityEngine.Vector3)(coherenceSync.coherencePosition); }
            set { coherenceSync.coherencePosition = (UnityEngine.Vector3)(value); }
        }

        protected override UnityEngine.Vector3 ReadComponentData(ICoherenceComponentData coherenceComponent, Vector3 floatingOriginDelta)
        {
            var value = ((WorldPosition)coherenceComponent).value;
            if (!coherenceSync.HasParentWithCoherenceSync) { value += floatingOriginDelta; }
            
            return value;
        }

        public override ICoherenceComponentData WriteComponentData(ICoherenceComponentData coherenceComponent, double time)
        {
            var update = (WorldPosition)coherenceComponent;
            if (RuntimeInterpolationSettings.IsInterpolationNone)
            {
                update.value = Value;
            }
            else
            {
                update.value = GetInterpolatedAt(time);
            }
            
            return update;
        }

        public override ICoherenceComponentData CreateComponentData()
        {
            return new WorldPosition();
        }    
    }
    
    public class Binding_d75e47307ad1c5041b867c84e5260a80_595e0ae7_d227_41b7_8666_825dc641c72f : RotationBinding
    {   
        private global::UnityEngine.Transform CastedUnityComponent;

        protected override void OnBindingCloned()
        {
    	    CastedUnityComponent = (global::UnityEngine.Transform)UnityComponent;
        }

        public override string CoherenceComponentName => "WorldOrientation";
        public override uint FieldMask => 0b00000000000000000000000000000001;

        public override UnityEngine.Quaternion Value
        {
            get { return (UnityEngine.Quaternion)(coherenceSync.coherenceRotation); }
            set { coherenceSync.coherenceRotation = (UnityEngine.Quaternion)(value); }
        }

        protected override UnityEngine.Quaternion ReadComponentData(ICoherenceComponentData coherenceComponent, Vector3 floatingOriginDelta)
        {
            var value = ((WorldOrientation)coherenceComponent).value;
            
            return value;
        }

        public override ICoherenceComponentData WriteComponentData(ICoherenceComponentData coherenceComponent, double time)
        {
            var update = (WorldOrientation)coherenceComponent;
            if (RuntimeInterpolationSettings.IsInterpolationNone)
            {
                update.value = Value;
            }
            else
            {
                update.value = GetInterpolatedAt(time);
            }
            
            return update;
        }

        public override ICoherenceComponentData CreateComponentData()
        {
            return new WorldOrientation();
        }    
    }
    
    public class Binding_d75e47307ad1c5041b867c84e5260a80_219d4272_7cc0_4b82_a3b0_a189abcf3acd : BoolAnimatorParameterBinding
    {   
        private global::UnityEngine.Animator CastedUnityComponent;

        protected override void OnBindingCloned()
        {
    	    CastedUnityComponent = (global::UnityEngine.Animator)UnityComponent;
        }

        public override string CoherenceComponentName => "Player_d75e47307ad1c5041b867c84e5260a80_Animator_7456888245380698815";
        public override uint FieldMask => 0b00000000000000000000000000000001;

        public override System.Boolean Value
        {
            get { return (System.Boolean)(CastedUnityComponent.GetBool(CastedDescriptor.ParameterHash)); }
            set { CastedUnityComponent.SetBool(CastedDescriptor.ParameterHash, value); }
        }

        protected override System.Boolean ReadComponentData(ICoherenceComponentData coherenceComponent, Vector3 floatingOriginDelta)
        {
            var value = ((Player_d75e47307ad1c5041b867c84e5260a80_Animator_7456888245380698815)coherenceComponent).Walking;
            
            return value;
        }

        public override ICoherenceComponentData WriteComponentData(ICoherenceComponentData coherenceComponent, double time)
        {
            var update = (Player_d75e47307ad1c5041b867c84e5260a80_Animator_7456888245380698815)coherenceComponent;
            if (RuntimeInterpolationSettings.IsInterpolationNone)
            {
                update.Walking = Value;
            }
            else
            {
                update.Walking = GetInterpolatedAt(time);
            }
            
            return update;
        }

        public override ICoherenceComponentData CreateComponentData()
        {
            return new Player_d75e47307ad1c5041b867c84e5260a80_Animator_7456888245380698815();
        }    
    }
    
    public class Binding_d75e47307ad1c5041b867c84e5260a80_a8de01fe_f7e3_4c3a_a268_096800c34994 : StringBinding
    {   
        private global::NetworkCharacter CastedUnityComponent;

        protected override void OnBindingCloned()
        {
    	    CastedUnityComponent = (global::NetworkCharacter)UnityComponent;
        }

        public override string CoherenceComponentName => "Player_d75e47307ad1c5041b867c84e5260a80_NetworkCharacter_6071707303727537263";
        public override uint FieldMask => 0b00000000000000000000000000000001;

        public override System.String Value
        {
            get { return (System.String)(CastedUnityComponent.avatarModelID); }
            set { CastedUnityComponent.avatarModelID = (System.String)(value); }
        }

        protected override System.String ReadComponentData(ICoherenceComponentData coherenceComponent, Vector3 floatingOriginDelta)
        {
            var value = ((Player_d75e47307ad1c5041b867c84e5260a80_NetworkCharacter_6071707303727537263)coherenceComponent).avatarModelID;
            
            return value;
        }

        public override ICoherenceComponentData WriteComponentData(ICoherenceComponentData coherenceComponent, double time)
        {
            var update = (Player_d75e47307ad1c5041b867c84e5260a80_NetworkCharacter_6071707303727537263)coherenceComponent;
            if (RuntimeInterpolationSettings.IsInterpolationNone)
            {
                update.avatarModelID = Value;
            }
            else
            {
                update.avatarModelID = GetInterpolatedAt(time);
            }
            
            return update;
        }

        public override ICoherenceComponentData CreateComponentData()
        {
            return new Player_d75e47307ad1c5041b867c84e5260a80_NetworkCharacter_6071707303727537263();
        }    
    }

    public class CoherenceSyncPlayer_d75e47307ad1c5041b867c84e5260a80 : CoherenceSyncBaked
    {
        private Entity entityId;
        private Logger logger = Coherence.Log.Log.GetLogger<CoherenceSyncPlayer_d75e47307ad1c5041b867c84e5260a80>();
        
        
        
        private IClient client;
        private CoherenceBridge bridge;
        
        private readonly Dictionary<string, Binding> bakedValueBindings = new Dictionary<string, Binding>()
        {
            ["4611e264-3ddd-458c-8f2a-444cd5794bad"] = new Binding_d75e47307ad1c5041b867c84e5260a80_4611e264_3ddd_458c_8f2a_444cd5794bad(),
            ["595e0ae7-d227-41b7-8666-825dc641c72f"] = new Binding_d75e47307ad1c5041b867c84e5260a80_595e0ae7_d227_41b7_8666_825dc641c72f(),
            ["219d4272-7cc0-4b82-a3b0-a189abcf3acd"] = new Binding_d75e47307ad1c5041b867c84e5260a80_219d4272_7cc0_4b82_a3b0_a189abcf3acd(),
            ["a8de01fe-f7e3-4c3a-a268-096800c34994"] = new Binding_d75e47307ad1c5041b867c84e5260a80_a8de01fe_f7e3_4c3a_a268_096800c34994(),
        };
        
        private Dictionary<string, Action<CommandBinding, CommandsHandler>> bakedCommandBindings = new Dictionary<string, Action<CommandBinding, CommandsHandler>>();
        
        public CoherenceSyncPlayer_d75e47307ad1c5041b867c84e5260a80()
        {
        }
        
        public override Binding BakeValueBinding(Binding valueBinding)
        {
            if (bakedValueBindings.TryGetValue(valueBinding.guid, out var bakedBinding))
            {
                valueBinding.CloneTo(bakedBinding);
                return bakedBinding;
            }
            
            return null;
        }
        
        public override void BakeCommandBinding(CommandBinding commandBinding, CommandsHandler commandsHandler)
        {
            if (bakedCommandBindings.TryGetValue(commandBinding.guid, out var commandBindingBaker))
            {
                commandBindingBaker.Invoke(commandBinding, commandsHandler);
            }
        }
        
        public override void ReceiveCommand(IEntityCommand command)
        {
            switch (command)
            {
                default:
                    logger.Warning($"CoherenceSyncPlayer_d75e47307ad1c5041b867c84e5260a80 Unhandled command: {command.GetType()}.");
                    break;
            }
        }
        
        public override List<ICoherenceComponentData> CreateEntity(bool usesLodsAtRuntime, string archetypeName)
        {
            if (!usesLodsAtRuntime)
            {
                return null;
            }
            
            if (Archetypes.IndexForName.TryGetValue(archetypeName, out int archetypeIndex))
            {
                var components = new List<ICoherenceComponentData>()
                {
                    new ArchetypeComponent
                    {
                        index = archetypeIndex
                    }
                };

                return components;
            }
    
            logger.Warning($"Unable to find archetype {archetypeName} in dictionary. Please, bake manually (coherence > Bake)");
            
            return null;
        }
        
        public override void Dispose()
        {
        }
        
        public override void Initialize(Entity entityId, CoherenceBridge bridge, IClient client, CoherenceInput input, Logger logger)
        {
            this.logger = logger.With<CoherenceSyncPlayer_d75e47307ad1c5041b867c84e5260a80>();
            this.bridge = bridge;
            this.entityId = entityId;
            this.client = client;        
        }
    }

}