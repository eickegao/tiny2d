using System;

namespace Microsoft.Xna.Framework.Content
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class ContentSerializerAttribute : Attribute
	{
		#region Private Member Variables

		private bool allowNull;
		private string collectionItemName;
		private string elementName;
		private bool flattenContent;
		private bool hasCollectionItemName;
		private bool optional;
		private bool sharedResource;

		#endregion


		#region Public Constructors

		public ContentSerializerAttribute()
		{
			//empty 
		}

		#endregion Public Constructors


		#region Public Properties

		public bool AllowNull
		{
			get { return this.allowNull; }
			set { this.allowNull = value; }
		}

		public string CollectionItemName
		{
			get { return this.collectionItemName; }
			set { this.collectionItemName = value; }
		}

		public string ElementName
		{
			get { return this.elementName; }
			set { this.elementName = value; }
		}

		public bool FlattenContent
		{
			get { return this.flattenContent; }
			set { this.flattenContent = value; }
		}

		public bool HasCollectionItemName
		{
			get { return this.hasCollectionItemName; }
		}

		public bool Optional
		{
			get { return this.optional; }
			set { this.optional = value; }
		}

		public bool SharedResource
		{
			get { return this.sharedResource; }
			set { this.sharedResource = value; }
		}


		public ContentSerializerAttribute Clone()
		{
			ContentSerializerAttribute clone = new ContentSerializerAttribute();
			clone.allowNull = this.allowNull;
			clone.collectionItemName = this.collectionItemName;
			clone.elementName = this.elementName;
			clone.flattenContent = this.flattenContent;
			clone.hasCollectionItemName = this.hasCollectionItemName;
			clone.optional = this.optional;
			clone.sharedResource = this.sharedResource;
			return clone;
		}

		#endregion Public Properties
	}
} 
