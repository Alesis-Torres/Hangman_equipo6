﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HangmanServerLibrary.GameServiceReference {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="WordDTO", Namespace="http://schemas.datacontract.org/2004/07/Hangman_Server.Model.DTO")]
    [System.SerializableAttribute()]
    public partial class WordDTO : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int IdField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private byte[] ImageBytesField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string NameField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int Id {
            get {
                return this.IdField;
            }
            set {
                if ((this.IdField.Equals(value) != true)) {
                    this.IdField = value;
                    this.RaisePropertyChanged("Id");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public byte[] ImageBytes {
            get {
                return this.ImageBytesField;
            }
            set {
                if ((object.ReferenceEquals(this.ImageBytesField, value) != true)) {
                    this.ImageBytesField = value;
                    this.RaisePropertyChanged("ImageBytes");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Name {
            get {
                return this.NameField;
            }
            set {
                if ((object.ReferenceEquals(this.NameField, value) != true)) {
                    this.NameField = value;
                    this.RaisePropertyChanged("Name");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CategoryDTO", Namespace="http://schemas.datacontract.org/2004/07/Hangman_Server.Model.DTO")]
    [System.SerializableAttribute()]
    public partial class CategoryDTO : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private byte[] ImageBytesField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string NameField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public byte[] ImageBytes {
            get {
                return this.ImageBytesField;
            }
            set {
                if ((object.ReferenceEquals(this.ImageBytesField, value) != true)) {
                    this.ImageBytesField = value;
                    this.RaisePropertyChanged("ImageBytes");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Name {
            get {
                return this.NameField;
            }
            set {
                if ((object.ReferenceEquals(this.NameField, value) != true)) {
                    this.NameField = value;
                    this.RaisePropertyChanged("Name");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="GameServiceReference.IGameService")]
    public interface IGameService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IGameService/ProbarConexion", ReplyAction="http://tempuri.org/IGameService/ProbarConexionResponse")]
        string ProbarConexion();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IGameService/ProbarConexion", ReplyAction="http://tempuri.org/IGameService/ProbarConexionResponse")]
        System.Threading.Tasks.Task<string> ProbarConexionAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IGameService/RegistrarPartidaInconclusa", ReplyAction="http://tempuri.org/IGameService/RegistrarPartidaInconclusaResponse")]
        void RegistrarPartidaInconclusa(int salaId, int idChallenger, int idGuesser, int idPalabra, int idDesconectado, string codigoSala);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IGameService/RegistrarPartidaInconclusa", ReplyAction="http://tempuri.org/IGameService/RegistrarPartidaInconclusaResponse")]
        System.Threading.Tasks.Task RegistrarPartidaInconclusaAsync(int salaId, int idChallenger, int idGuesser, int idPalabra, int idDesconectado, string codigoSala);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IGameService/RegistrarPartidaFinalizada", ReplyAction="http://tempuri.org/IGameService/RegistrarPartidaFinalizadaResponse")]
        void RegistrarPartidaFinalizada(int salaId, int idChallenger, int idGuesser, int idPalabra, int idDesconectado, string codigoSala);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IGameService/RegistrarPartidaFinalizada", ReplyAction="http://tempuri.org/IGameService/RegistrarPartidaFinalizadaResponse")]
        System.Threading.Tasks.Task RegistrarPartidaFinalizadaAsync(int salaId, int idChallenger, int idGuesser, int idPalabra, int idDesconectado, string codigoSala);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IGameService/ObtenerPalabrasPorCategoria", ReplyAction="http://tempuri.org/IGameService/ObtenerPalabrasPorCategoriaResponse")]
        HangmanServerLibrary.GameServiceReference.WordDTO[] ObtenerPalabrasPorCategoria(string categoria, int idLenguaje);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IGameService/ObtenerPalabrasPorCategoria", ReplyAction="http://tempuri.org/IGameService/ObtenerPalabrasPorCategoriaResponse")]
        System.Threading.Tasks.Task<HangmanServerLibrary.GameServiceReference.WordDTO[]> ObtenerPalabrasPorCategoriaAsync(string categoria, int idLenguaje);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IGameService/ObtenerCategorias", ReplyAction="http://tempuri.org/IGameService/ObtenerCategoriasResponse")]
        HangmanServerLibrary.GameServiceReference.CategoryDTO[] ObtenerCategorias(int idLenguaje);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IGameService/ObtenerCategorias", ReplyAction="http://tempuri.org/IGameService/ObtenerCategoriasResponse")]
        System.Threading.Tasks.Task<HangmanServerLibrary.GameServiceReference.CategoryDTO[]> ObtenerCategoriasAsync(int idLenguaje);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IGameServiceChannel : HangmanServerLibrary.GameServiceReference.IGameService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class GameServiceClient : System.ServiceModel.ClientBase<HangmanServerLibrary.GameServiceReference.IGameService>, HangmanServerLibrary.GameServiceReference.IGameService {
        
        public GameServiceClient() {
        }
        
        public GameServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public GameServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public GameServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public GameServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public string ProbarConexion() {
            return base.Channel.ProbarConexion();
        }
        
        public System.Threading.Tasks.Task<string> ProbarConexionAsync() {
            return base.Channel.ProbarConexionAsync();
        }
        
        public void RegistrarPartidaInconclusa(int salaId, int idChallenger, int idGuesser, int idPalabra, int idDesconectado, string codigoSala) {
            base.Channel.RegistrarPartidaInconclusa(salaId, idChallenger, idGuesser, idPalabra, idDesconectado, codigoSala);
        }
        
        public System.Threading.Tasks.Task RegistrarPartidaInconclusaAsync(int salaId, int idChallenger, int idGuesser, int idPalabra, int idDesconectado, string codigoSala) {
            return base.Channel.RegistrarPartidaInconclusaAsync(salaId, idChallenger, idGuesser, idPalabra, idDesconectado, codigoSala);
        }
        
        public void RegistrarPartidaFinalizada(int salaId, int idChallenger, int idGuesser, int idPalabra, int idDesconectado, string codigoSala) {
            base.Channel.RegistrarPartidaFinalizada(salaId, idChallenger, idGuesser, idPalabra, idDesconectado, codigoSala);
        }
        
        public System.Threading.Tasks.Task RegistrarPartidaFinalizadaAsync(int salaId, int idChallenger, int idGuesser, int idPalabra, int idDesconectado, string codigoSala) {
            return base.Channel.RegistrarPartidaFinalizadaAsync(salaId, idChallenger, idGuesser, idPalabra, idDesconectado, codigoSala);
        }
        
        public HangmanServerLibrary.GameServiceReference.WordDTO[] ObtenerPalabrasPorCategoria(string categoria, int idLenguaje) {
            return base.Channel.ObtenerPalabrasPorCategoria(categoria, idLenguaje);
        }
        
        public System.Threading.Tasks.Task<HangmanServerLibrary.GameServiceReference.WordDTO[]> ObtenerPalabrasPorCategoriaAsync(string categoria, int idLenguaje) {
            return base.Channel.ObtenerPalabrasPorCategoriaAsync(categoria, idLenguaje);
        }
        
        public HangmanServerLibrary.GameServiceReference.CategoryDTO[] ObtenerCategorias(int idLenguaje) {
            return base.Channel.ObtenerCategorias(idLenguaje);
        }
        
        public System.Threading.Tasks.Task<HangmanServerLibrary.GameServiceReference.CategoryDTO[]> ObtenerCategoriasAsync(int idLenguaje) {
            return base.Channel.ObtenerCategoriasAsync(idLenguaje);
        }
    }
}
