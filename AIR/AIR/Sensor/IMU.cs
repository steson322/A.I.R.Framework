using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AIR.Maths;

namespace AIR.Sensor
{
    public class IMU : Sensor<IMUPackage>
    {
        public string debug = "";

        #region Public Property
        /// <summary>
        /// Accelerometer Sensitivity
        /// </summary>
        public const double AccelSensitivity = 8192.0 / 2.0;
        /// <summary>
        /// Gyro Sensitivity
        /// </summary>
        public const double GyroSensitivity = 13.154;
        /// <summary>
        /// SamplePeriod
        /// </summary>
        public const double SamplePeriod = 0.05;
        /// <summary>
        /// Calculate Gyro constant
        /// </summary>
        public const double GyroConstant = SamplePeriod * Math.PI / 180.0 / GyroSensitivity;
        /// <summary>
        /// Raw data of IMU
        /// </summary>
        public IMUPackage RawData { get; private set; }
        /// <summary>
        /// Pitch angle
        /// </summary>
        public double Pitch { get; private set; }
        /// <summary>
        /// roll angle
        /// </summary>
        public double Roll { get; private set; }
        /// <summary>
        /// turn angle
        /// </summary>
        public double Heading { get; private set; }
        /// <summary>
        /// X displacement
        /// </summary>
        public double LocationX { get; private set; }
        /// <summary>
        /// Y displacement
        /// </summary>
        public double LocationY { get; private set; }
        /// <summary>
        /// Z displacement
        /// </summary>
        public double LocationZ { get; private set; }
        /// <summary>
        /// Use calibration or not
        /// </summary>
        public bool UsingCalibration = false;
        /// <summary>
        /// Ratio of measurement in place of filtering;
        /// </summary>
        public double ComplementaryFilterRatio = 0.7;
        #endregion Public Property

        #region Private Property
        int CalibrateCount = 0;
        int CalibrateSample = 512;
        public double GyroXAvg = 0;
        public double GyroXMax = Double.MinValue;
        public double GyroXMin = Double.MaxValue;
        public double GyroYAvg = 0;
        public double GyroYMax = Double.MinValue;
        public double GyroYMin = Double.MaxValue;
        public double GyroZAvg = 0;
        public double GyroZMax = Double.MinValue;
        public double GyroZMin = Double.MaxValue;
        
        public double MagXSca = 490;//520.5;
        public double MagXOff = -78;//170.5;
        public double MagXMax = Double.MinValue;
        public double MagXMin = Double.MaxValue;
        public double MagYSca = 500;//485;
        public double MagYOff = -132;//-12;
        public double MagYMax = Double.MinValue;
        public double MagYMin = Double.MaxValue;
        public double MagZSca = 460.5;//496;
        public double MagZOff = -20.5;//129;
        public double MagZMax = Double.MinValue;
        public double MagZMin = Double.MaxValue;

        public double MagXCali = 0;
        public double MagYCali = 0;
        public double MagZCali = 0;
        #endregion Private Property

        /// <summary>
        /// Constructor of a  IMU object
        /// </summary>
        public IMU()
        {
            RawData = new IMUPackage();
            Heading = 0;
        }
        /// <summary>
        /// Update a IMU data
        /// </summary>
        /// <param name="package"></param>
        public override void Update(IMUPackage package)
        {
            //UsingCalibration = true;
            this.RawData = package;
            if (UsingCalibration)
            {
                if (CalibrateCount < CalibrateSample)
                {
                    MagXMax = RawData.MagX > MagXMax ? RawData.MagX : MagXMax;
                    MagXMin = RawData.MagX < MagXMin ? RawData.MagX : MagXMin;
                    MagYMax = RawData.MagY > MagYMax ? RawData.MagY : MagYMax;
                    MagYMin = RawData.MagY < MagYMin ? RawData.MagY : MagYMin;
                    MagZMax = RawData.MagZ > MagZMax ? RawData.MagZ : MagZMax;
                    MagZMin = RawData.MagZ < MagZMin ? RawData.MagZ : MagZMin;
                    CalibrateCount++;

                    string debugOut = "MagX = " + MagXMin + " << " + MagXMax;
                    debugOut += "MagY = " + MagYMin + " << " + MagYMax;
                    debugOut += "MagZ = " + MagZMin + " << " + MagZMax;
                    this.debug = debugOut;

                    return;
                }
                else if (CalibrateCount == CalibrateSample)
                {
                    MagXOff = (MagXMax + MagXMin) / 2.0;
                    MagXSca = MagXMax - MagXOff;
                    MagYOff = (MagYMax + MagYMin) / 2.0;
                    MagYSca = MagYMax - MagYOff;
                    MagZOff = (MagZMax + MagZMin) / 2.0;
                    MagZSca = MagZMax - MagZOff;
                    CalibrateCount++;
                    Calibrated = true;

                    string debugOut = "MagX Off|Scale = " + MagXOff + " | " + MagXSca;
                    debugOut += "MagY Off|Scale = " + MagYOff + " << " + MagYSca;
                    debugOut += "MagZ Off|Scale = " + MagZOff + " << " + MagZSca;
                    this.debug = debugOut;
                }
            }
            else
            {
                Calibrated = true;
            }

            //Sensor Fusion

            if (Calibrated)
            {
                double tempAccelX = RawData.AccelX;
                double tempAccelY = -RawData.AccelY;
                double tempAccelZ = RawData.AccelZ;
                double tempGyroX = RawData.GyroX - GyroXAvg;
                double tempGyroY = RawData.GyroY - GyroYAvg;
                double tempGyroZ = -RawData.GyroZ + GyroZAvg;
                double tempMagX = (RawData.MagY - MagYOff) / MagYSca;
                double tempMagY = (MagXOff - RawData.MagX) / MagXSca;
                double tempMagZ = (RawData.MagZ - MagZOff) / MagZSca;

                double MeaRoll = Math.PI + Math.Atan2(tempAccelX, tempAccelZ);
                double MeaPitch = -Math.Atan2(tempAccelY, Math.Sqrt(tempAccelX * tempAccelX + tempAccelZ * tempAccelZ));
                double EstRoll = Roll + Math.Cos(Pitch) * tempGyroY * GyroConstant
                                      - Math.Sin(Pitch) * tempGyroZ * GyroConstant;
                double EstPitch = Pitch + Math.Cos(Roll) * tempGyroX * GyroConstant
                                        - Math.Sin(Roll) * tempGyroZ * GyroConstant;
                double EstHeading = Heading
                    + 0.5 *
                    (Math.Cos(Roll) * tempGyroZ * GyroConstant
                    + Math.Cos(Pitch) * tempGyroZ * GyroConstant
                    - Math.Sin(Roll) * tempGyroX * GyroConstant
                    - Math.Sin(Pitch) * tempGyroY * GyroConstant);

                //refine data
                while (MeaPitch < -Math.PI)
                    MeaPitch += Math.PI * 2.0;
                while (MeaPitch > Math.PI)
                    MeaPitch -= Math.PI * 2.0;
                while (MeaRoll < -Math.PI)
                    MeaRoll += Math.PI * 2.0;
                while (MeaRoll > Math.PI)
                    MeaRoll -= Math.PI * 2.0;

                //refine data
                while (EstPitch < -Math.PI)
                    EstPitch += Math.PI * 2.0;
                while (EstPitch > Math.PI)
                    EstPitch -= Math.PI * 2.0;
                while (EstRoll < -Math.PI)
                    EstRoll += Math.PI * 2.0;
                while (EstRoll > Math.PI)
                    EstRoll -= Math.PI * 2.0;

                bool AutoGain = false;
                double tempRoll;
                double tempPitch;
                if (AutoGain)
                {
                    //new
                    //Adjust Complementary Filter gain depend on data difference
                    double RollComplementaryFilterRatio = Math.Abs(EstRoll - MeaRoll) / (Math.PI / 2.0);
                    double PitchComplementaryFilterRatio = Math.Abs(EstPitch - MeaPitch) / (Math.PI / 2.0);
                    RollComplementaryFilterRatio = RollComplementaryFilterRatio > 1.0 ? 1.0 : RollComplementaryFilterRatio;
                    PitchComplementaryFilterRatio = PitchComplementaryFilterRatio > 1.0 ? 1.0 : PitchComplementaryFilterRatio;
                    double RollComplementaryFilterRatioInv = 1.0 - RollComplementaryFilterRatio;
                    double PitchComplementaryFilterRatioInv = 1.0 - PitchComplementaryFilterRatio;
                    tempRoll = Maths.Utility.AdjustAngle(MeaRoll, RollComplementaryFilterRatioInv, EstRoll, RollComplementaryFilterRatio);
                    tempPitch = Maths.Utility.AdjustAngle(MeaPitch, PitchComplementaryFilterRatioInv, EstPitch, PitchComplementaryFilterRatio);
                    //new
                }
                else
                {
                    //old 
                    //correct complementary filter gain
                    double ComplementaryFilterRatioInv = 1.0 - ComplementaryFilterRatio;
                    tempRoll = Maths.Utility.AdjustAngle(MeaRoll, ComplementaryFilterRatio, EstRoll, ComplementaryFilterRatioInv);
                    tempPitch = Maths.Utility.AdjustAngle(MeaPitch, ComplementaryFilterRatio, EstPitch, ComplementaryFilterRatioInv);
                    //old
                }

                //refine data
                while (tempPitch < -Math.PI)
                    tempPitch += Math.PI * 2.0;
                while (tempPitch > Math.PI)
                    tempPitch -= Math.PI * 2.0;
                while (tempRoll < -Math.PI)
                    tempRoll += Math.PI * 2.0;
                while (tempRoll > Math.PI)
                    tempRoll -= Math.PI * 2.0;

                //Mag Fusion
                double RollCorrectionAngle = tempRoll * Math.Cos(tempPitch);
                Vector MagVector = new Vector(tempMagX, tempMagY, tempMagZ);
                Vector HeadingVector = new Vector(0, Math.Cos(tempPitch), Math.Sin(tempPitch));
                //correct roll angle of mag
                MagVector.Rotate(HeadingVector, RollCorrectionAngle);
                //correct pitch angle of mag
                MagVector.Rotate(new Vector(1, 0, 0), tempPitch);
                //forward calibrated
                MagXCali = MagVector.DirectionX;
                MagYCali = MagVector.DirectionY;
                MagZCali = MagVector.DirectionZ;
                //calculate heading
                double MeaHeading = -Math.Atan2(MagVector.DirectionX, MagVector.DirectionY);

                //refine data
                while (MeaHeading < -Math.PI)
                    MeaHeading += Math.PI * 2.0;
                while (MeaHeading > Math.PI)
                    MeaHeading -= Math.PI * 2.0;

                double tempHeading;
                if (AutoGain)
                {
                    //new
                    //Adjust Complementary Filter gain depend on data difference
                    double HeadingComplementaryFilterRatio = Math.Abs(EstHeading - MeaHeading) / (Math.PI / 2.0);
                    HeadingComplementaryFilterRatio = HeadingComplementaryFilterRatio > 1.0 ? 1.0 : HeadingComplementaryFilterRatio;
                    double HeadingComplementaryFilterRatioInv = 1.0 - HeadingComplementaryFilterRatio;
                    tempHeading = Maths.Utility.AdjustAngle(MeaHeading, HeadingComplementaryFilterRatioInv, EstHeading, HeadingComplementaryFilterRatio);
                    //new
                }
                else
                {
                    //old 
                    double ComplementaryFilterRatio2 = ComplementaryFilterRatio / 2.0;
                    //correct complementary filter gain
                    double ComplementaryFilterRatioInv2 = 1.0 - ComplementaryFilterRatio2;
                    tempHeading = Maths.Utility.AdjustAngle(MeaHeading, ComplementaryFilterRatio2, EstHeading, ComplementaryFilterRatioInv2);
                    //old
                }

                //refine data
                while (tempHeading < -Math.PI)
                    tempHeading += Math.PI * 2.0;
                while (tempHeading > Math.PI)
                    tempHeading -= Math.PI * 2.0;

                //append data
                Pitch = tempPitch;
                Roll = tempRoll;
                Heading = tempHeading;

                //time stamp
                this.LastUpdateTime = DateTime.Now;
            }
        }
        /// <summary>
        /// Read a byte array to IMU package data
        /// </summary>
        /// <param name="XML"></param>
        public static IMUPackage ReadPackage(byte[] bytes)
        {
            IMUPackage package = new IMUPackage();
            try
            {
                if (bytes.Length < 27)
                {
                    package.AccelX = BitConverter.ToInt16(bytes, 0);
                    package.AccelY = BitConverter.ToInt16(bytes, 3);
                    package.AccelZ = -BitConverter.ToInt16(bytes, 6);
                    package.GyroX = BitConverter.ToInt16(bytes, 9);
                    package.GyroY = BitConverter.ToInt16(bytes, 12);
                    package.GyroZ = BitConverter.ToInt16(bytes, 15);
                    package.MagX = BitConverter.ToInt16(bytes, 18);
                    package.MagY = BitConverter.ToInt16(bytes, 21);
                    package.MagZ = BitConverter.ToInt16(bytes, 24);
                }
            }
            catch (ArgumentOutOfRangeException)
            { }
            return package;
        }
    }

    /// <summary>
    /// A package of IMU data
    /// </summary>
    public class IMUPackage : SensorPackage
    {
        public long AccelX, AccelY, AccelZ;
        public long GyroX, GyroY, GyroZ;
        public long MagX, MagY, MagZ;
        public double Temperature;
    }
}
