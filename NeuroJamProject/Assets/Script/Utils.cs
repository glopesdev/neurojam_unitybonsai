using Bonsai;
using Bonsai.Design;
using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;



    public class Utils
    {
        public static bool Ping(string IP)
        {
            bool result = false;
            Ping p = new Ping();
            try
            {
                PingReply reply = p.Send(IP, 2000);
                if (reply.Status == IPStatus.Success)
                    return true;
            }
            catch { }
            return result;
        }

        public static Enum GetEnumValue<T>(string description)
        {
            Type enumType = typeof(T);
            Array enumValArray = Enum.GetValues(enumType);

            foreach (Enum value in enumValArray)
            {
                if (description == GetEnumDescription(value))
                    return value;
            }

            return null;
        }

        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }
    /*
        public static IDisposable RegisterVisualizer<TSource>(IObservable<TSource> source, DialogTypeVisualizer visualizer, VisualizerPanel panel)
        {
            visualizer.Load(panel);
            var visualizerObservable = new ReplaySubject<IObservable<object>>();
            visualizerObservable.OnNext(source.Select(xs =>
            {
                return (object)xs;
            }));
            return visualizer.Visualize(visualizerObservable, panel).Subscribe();
        }*/
    /*
        public static Point2f[] InitializeQuadrangle(Size size)
        {
            return new[]
            {
                new Point2f(0, 0),
                new Point2f(0, size.Height),
                new Point2f(size.Width, size.Height),
                new Point2f(size.Width, 0)
            };
        }*/

        //TODO : This does not exist in bonsai
        public static object GetWorkflowProperty(ExpressionBuilderGraph source, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("The workflow property name cannot be null or whitespace.", "name");
            }

            var memberChain = name.Split(new[] { ExpressionHelper.MemberSeparator }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < memberChain.Length - 1; i++)
            {
                var workflowBuilders = (from node in source
                                        let builder = ExpressionBuilder.Unwrap(node.Value) as WorkflowExpressionBuilder
                                        where builder != null && builder.Name == memberChain[i]
                                        select builder).ToArray();
                if (workflowBuilders.Length == 0)
                {
                    throw new KeyNotFoundException(string.Format("Property not found", name));
                }
                else if (workflowBuilders.Length > 1)
                {
                    throw new InvalidOperationException(string.Format(
                        "Ambiguous named element",
                        string.Join(ExpressionHelper.MemberSeparator, memberChain, 0, i + 1)));
                }

                source = workflowBuilders[0].Workflow;
            }

            name = memberChain[memberChain.Length - 1];
            var property = (from node in source
                            let workflowProperty = ExpressionBuilder.Unwrap(node.Value) as ExternalizedProperty
                            where workflowProperty != null && workflowProperty.Name == name
                            select workflowProperty)
                            .FirstOrDefault();
            if (property == null)
            {
                throw new KeyNotFoundException(string.Format("Property not found", name));
            }

            var propertyDescriptor = TypeDescriptor.GetProperties(property).Find("Value", false);
            return propertyDescriptor.GetValue(property);
        }

        //TODO : This exist in Bonsai but does not work, check updates
        public static void SetWorkflowProperty(ExpressionBuilderGraph source, string name, object value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("The workflow property name cannot be null or whitespace.", "name");
            }

            var memberChain = name.Split(new[] { ExpressionHelper.MemberSeparator }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < memberChain.Length - 1; i++)
            {
                var workflowBuilders = (from node in source
                                        let builder = ExpressionBuilder.Unwrap(node.Value) as WorkflowExpressionBuilder
                                        where builder != null && builder.Name == memberChain[i]
                                        select builder).ToArray();
                if (workflowBuilders.Length == 0)
                {
                    throw new KeyNotFoundException(string.Format("Property not found", name));
                }
                else if (workflowBuilders.Length > 1)
                {
                    throw new InvalidOperationException(string.Format(
                       "Ambiguous named element",
                       string.Join(ExpressionHelper.MemberSeparator, memberChain, 0, i + 1)));
                }

                source = workflowBuilders[0].Workflow;
            }

            name = memberChain[memberChain.Length - 1];
            var property = (from node in source
                            let workflowProperty = ExpressionBuilder.Unwrap(node.Value) as ExternalizedProperty
                            where workflowProperty != null && workflowProperty.Name == name
                            select workflowProperty)
                            .FirstOrDefault();
            if (property == null)
            {
                throw new KeyNotFoundException(string.Format("Property not found", name));
            }

            var propertyDescriptor = TypeDescriptor.GetProperties(property).Find("Value", false);
            //var propertyValue = propertyDescriptor.Converter.ConvertFrom(value);
            propertyDescriptor.SetValue(property, value);
        }
    }

