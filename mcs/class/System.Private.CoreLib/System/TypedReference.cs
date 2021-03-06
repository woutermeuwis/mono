using System.Reflection;
using System.Runtime.CompilerServices;

namespace System
{
	public ref struct TypedReference
	{
		#region sync with object-internals.h
		RuntimeTypeHandle type;
		IntPtr Value;
		IntPtr Type;
		#endregion

		public static TypedReference MakeTypedReference (Object target, FieldInfo[] flds)
		{
			if (target == null)
				throw new ArgumentNullException (nameof (target));
			if (flds == null)
				throw new ArgumentNullException (nameof (flds));
			if (flds.Length == 0)
				throw new ArgumentException (SR.Arg_ArrayZeroError, nameof (flds));

			var fields = new IntPtr [flds.Length];
			var targetType = (RuntimeType)target.GetType ();
			for (int i = 0; i < flds.Length; i++) {
				var field = flds [i] as RuntimeFieldInfo;
				if (field == null)
					throw new ArgumentException (SR.Argument_MustBeRuntimeFieldInfo);
				if (field.IsStatic)
					throw new ArgumentException (SR.Argument_TypedReferenceInvalidField);

				if (targetType != field.GetDeclaringTypeInternal () && !targetType.IsSubclassOf (field.GetDeclaringTypeInternal ()))
					throw new MissingMemberException (SR.MissingMemberTypeRef);

				var fieldType = (RuntimeType)field.FieldType;
				if (fieldType.IsPrimitive)
					throw new ArgumentException (SR.Arg_TypeRefPrimitve);
				if (i < (flds.Length - 1) && !fieldType.IsValueType)
					throw new MissingMemberException (SR.MissingMemberNestErr);

				fields[i] = field.FieldHandle.Value;
				targetType = fieldType;
			}

			var result = new TypedReference ();

			unsafe {
				InternalMakeTypedReference (&result, target, fields, targetType);
			}
			return result;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		unsafe static extern void InternalMakeTypedReference (void* result, Object target, IntPtr[] flds, RuntimeType lastFieldType);

		public override int GetHashCode ()
		{
			if (Type == IntPtr.Zero)
				return 0;
			else
				return __reftype (this).GetHashCode ();
		}

		public override bool Equals (Object o)
		{
			throw new NotSupportedException (SR.NotSupported_NYI);
		}

		public unsafe static Object ToObject (TypedReference value)
		{
			return InternalToObject (&value);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		unsafe extern static Object InternalToObject (void * value);

		internal bool IsNull {
			get {
				return Value == IntPtr.Zero && Type == IntPtr.Zero;
			}
		}

		public static Type GetTargetType (TypedReference value)
		{
			return __reftype (value);
		}

		public static RuntimeTypeHandle TargetTypeToken (TypedReference value)
		{
			return __reftype (value).TypeHandle;
		}

		public unsafe static void SetTypedReference (TypedReference target, Object value)
		{
			throw new NotImplementedException ();
		}
	}
}
