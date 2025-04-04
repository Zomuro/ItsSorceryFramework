using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ItsSorceryFramework
{
    public static class EquationUtility
    {
        public static float CalculateEquation(float inputVar, float baseVal, float coeff = 1, Dictionary<int, float> powCoeffs = null, EquationType equationType = EquationType.Linear)
        {
            switch (equationType)
            {
                case EquationType.Constant:
                    return baseVal;
                
                case EquationType.Linear:
                    return LinearEquation(inputVar, baseVal, coeff);

                case EquationType.Polynomial:
                    return PolynomialEquation(inputVar, baseVal, coeff, powCoeffs);

                case EquationType.Exponential:
                    return ExponentialEquation(inputVar, baseVal, coeff);

                case EquationType.Power:
                    return PowerEquation(inputVar, baseVal, coeff);

                default:
                    break;
            }
            
            return 0f;
        }
        
        
        public static float LinearEquation(float inputVar, float baseVal, float coeff) => coeff * inputVar + baseVal;
        
        public static float PolynomialEquation(float inputVar, float baseVal, float coeff = 1, Dictionary<int, float> powCoeffs = null)
        {
            if (powCoeffs is null || powCoeffs.Count() == 0) return coeff * inputVar + baseVal;

            float total = baseVal;
            foreach (var p in powCoeffs) total += p.Value * Mathf.Pow(inputVar, p.Key);
            return total;
        }

        public static float ExponentialEquation(float inputVar, float baseVal, float coeff) => baseVal * Mathf.Pow(coeff, inputVar);

        public static float PowerEquation(float inputVar, float baseVal, float coeff) => baseVal * Mathf.Pow(inputVar, coeff);
    }

    public enum EquationType : byte
    {
        Constant,
        Linear,
        Polynomial,
        Exponential,
        Power
    }
}
