using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsIotLedDriver;

namespace GlowPi
{
    class LedController
    {
        TLC5947Controller m_controller;
        AnimatedLed[] m_leds;

        public LedController()
        {
            // Create the controller
            m_controller = new TLC5947Controller(6,13);
            m_controller.ToggleAnimation(true);

            // Create the LED array
            m_leds = new AnimatedLed[5];

            // Create the LEDs
            m_leds[0] = new AnimatedLed(LedType.RBG, true);
            m_leds[1] = new AnimatedLed(LedType.RBG, true);
            m_leds[2] = new AnimatedLed(LedType.RBG, true);
            m_leds[3] = new AnimatedLed(LedType.RBG, true);
            m_leds[4] = new AnimatedLed(LedType.RBG, true);

            // Associate the LEDs
            m_controller.AssoicateLed(0, m_leds[0].GetLed());
            m_controller.AssoicateLed(3, m_leds[1].GetLed());
            m_controller.AssoicateLed(6, m_leds[2].GetLed());
            m_controller.AssoicateLed(9, m_leds[3].GetLed());
            m_controller.AssoicateLed(15, m_leds[4].GetLed());
        }

        public AnimatedLed GetLed(int ledNumber)
        {
            if(ledNumber > 4 || ledNumber < 0)
            {
                throw new ArgumentOutOfRangeException("The led must be between 0 and 4!");
            }

            return m_leds[ledNumber];
        }

        public void AnimateMasterIntensity(double intensity, TimeSpan animationTime)
        {
            m_controller.AnimateMasterIntensity(intensity, animationTime);
        }

        public double GetMasterIntensity()
        {
            return m_controller.GetMasterIntensity();
        }

        public bool IsMasterIntensityAnimating()
        {
            return m_controller.IsMasterIntensityAnimating();
        }
    }
}
