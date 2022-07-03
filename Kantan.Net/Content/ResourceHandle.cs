using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using Kantan.Files;
using Kantan.Net.Utilities;
using Kantan.Utilities;

namespace Kantan.Net.Content
{
	public abstract class ResourceHandle : IDisposable
	{
		public Stream Stream { get; protected set; }

		public byte[] Header { get; protected set; }

		public string SuppliedType { get; protected set; }

		public string ComputedType { get; protected set; }

		public List<FileType> ResolvedTypes { get; protected set; }

		public string Value { get; protected set; }

		public bool IsBinaryType => ComputedType == FileType.MT_APPLICATION_OCTET_STREAM;

		public bool IsFile { get; protected set; }

		[MNNW(true, nameof(HttpResourceHandle.Response))]
		[MNNW(true, nameof(SuppliedType))]
		public bool IsUri { get; protected set; }

		public bool IsBinary => (IsFile || IsUri) && IsBinaryType;

		public bool Resolved { get; protected set; }

		protected ResourceHandle() { }

		public virtual List<FileType> Resolve(bool runExtra = false, IFileTypeResolver extraResolver = null)
		{
			if (Resolved) {
				return ResolvedTypes;
			}

			if (IsBinaryType) {
				// todo ...
			}


			if (runExtra) {

				extraResolver ??= IFileTypeResolver.Default;

				string rx = extraResolver.Resolve(Stream);

				var type = new FileType()
				{
					Type = rx
				};

				ResolvedTypes.Add(type);
			}

			Resolved = true;

			return ResolvedTypes;
		}

		#region Implementation of IDisposable

		public abstract void Dispose();

		#endregion

		

		public static async Task<ResourceHandle> GetAsync(string u, bool auto = false)
		{
			//todo: error handling


			Stream stream;

			ResourceHandle resource = null;


			bool isFile = File.Exists(u),
			     isUri  = UriUtilities.IsUri(u, out var uu);


			if (isFile) {
				stream = File.OpenRead(u);

				resource = new FileResourceHandle()
				{
					Stream = stream,
				
					
				};
			}
			else if (isUri) {
				var response = (await HttpUtilities.TryGetResponseAsync(u)) as IFlurlResponse;
				stream = await response.GetStreamAsync();

				resource = new HttpResourceHandle()
				{
					Response     = response,
					SuppliedType = response.GetSuppliedType(out var cbf),
					NoSniffFlag = response.Headers.TryGetFirst("X-Content-Type-Options", out var x)
					              && x == "nosniff",
				};


			}
			else {
				return null;
			}

			resource.Stream = stream;
			var header = await resource.Stream.ReadHeaderAsync(FileType.RSRC_HEADER_LEN);
			resource.Header        = header;
			resource.ComputedType  = FileType.IsBinaryResource(header);
			resource.ResolvedTypes = new List<FileType>();

			resource.IsFile        = isFile;
			resource.IsUri         = isUri;
			resource.Resolved      = false;
			resource.Value         = u;

			// resource.Resolve(true);

			if (resource.ComputedType == FileType.MT_APPLICATION_OCTET_STREAM) {
				// resource.Resolve(true);
				//todo...

				var rg = FileType.Resolve(resource.Header);
				resource.ResolvedTypes.AddRange(rg);


				if (auto) {
					resource.Resolve();
				}
			}


			return resource;
		}
	}
}