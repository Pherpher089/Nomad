﻿using System;

namespace ExternPropertyAttributes
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class ReadOnlyAttribute : DrawerAttribute
	{

	}
}
