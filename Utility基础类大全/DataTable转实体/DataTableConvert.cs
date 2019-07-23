/******************************************
 * AUTHOR:          Rector
 * CREATEDON:       2018-09-26
 * OFFICIAL_SITE:    ������(https://codedefault.com)--רע.NET/.NET Core
 * ��Ȩ���У�����ɾ��
 ******************************************/

/*
 * �ο����ӣ�http://www.codeisbug.com/Doc/3/1113
 */
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;

namespace DncZeus.Api.Extensions
{
    /// <summary>
    /// ��datatableת��Ϊ���󼯺��б�List&lt;T&gt;
    /// </summary>
    public static class DataTableConvert
    {
        //��DataRowת��Ϊ�����ί������
        private delegate T Load<T>(DataRow dataRecord);

        //���ڹ���Emit��DataRow�л�ȡ�ֶεķ�����Ϣ
        private static readonly MethodInfo getValueMethod = typeof(DataRow).GetMethod("get_Item", new Type[] { typeof(int) });

        //���ڹ���Emit��DataRow���ж��Ƿ�Ϊ���еķ�����Ϣ
        private static readonly MethodInfo isDBNullMethod = typeof(DataRow).GetMethod("IsNull", new Type[] { typeof(int) });

        //ʹ���ֵ�洢ʵ��������Լ���֮��Ӧ��Emit���ɵ�ת������
        private static Dictionary<Type, Delegate> rowMapMethods = new Dictionary<Type, Delegate>();

        /// <summary>
        /// ��DataTableת���ɷ��Ͷ����б�
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(this DataTable dt)
        {
            List<T> list = new List<T>();
            if (dt == null)
                return list;

            //���� ί��Load<T>��һ��ʵ��rowMap
            Load<T> rowMap = null;


            //��rowMapMethods���ҵ�ǰT���Ӧ��ת��������û����ʹ��Emit����һ����
            if (!rowMapMethods.ContainsKey(typeof(T)))
            {
                DynamicMethod method = new DynamicMethod("DynamicCreateEntity_" + typeof(T).Name, typeof(T), new Type[] { typeof(DataRow) }, typeof(T), true);
                ILGenerator generator = method.GetILGenerator();
                LocalBuilder result = generator.DeclareLocal(typeof(T));
                generator.Emit(OpCodes.Newobj, typeof(T).GetConstructor(Type.EmptyTypes));
                generator.Emit(OpCodes.Stloc, result);

                for (int index = 0; index < dt.Columns.Count; index++)
                {
                    PropertyInfo propertyInfo = typeof(T).GetProperty(dt.Columns[index].ColumnName);
                    Label endIfLabel = generator.DefineLabel();
                    if (propertyInfo != null && propertyInfo.GetSetMethod() != null)
                    {
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Ldc_I4, index);
                        generator.Emit(OpCodes.Callvirt, isDBNullMethod);
                        generator.Emit(OpCodes.Brtrue, endIfLabel);
                        generator.Emit(OpCodes.Ldloc, result);
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Ldc_I4, index);
                        generator.Emit(OpCodes.Callvirt, getValueMethod);
                        generator.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
                        generator.Emit(OpCodes.Callvirt, propertyInfo.GetSetMethod());
                        generator.MarkLabel(endIfLabel);
                    }
                }
                generator.Emit(OpCodes.Ldloc, result);
                generator.Emit(OpCodes.Ret);

                //��������Ժ󴫸�rowMap
                rowMap = (Load<T>)method.CreateDelegate(typeof(Load<T>));
            }
            else
            {
                rowMap = (Load<T>)rowMapMethods[typeof(T)];
            }

            //����Datatable��rows���ϣ�����rowMap��DataRowת��Ϊ����T��
            foreach (DataRow info in dt.Rows)
                list.Add(rowMap(info));
            return list;
        }
    }
}
