using System;





public static class MelSpectrogram
{
    public const int SampleRate = 16000;
    public const int ClipSamples = 16000;   
    public const int FrameLength = 400;     
    public const int FrameStep = 160;       
    public const int NFFT = 512;
    public const int NMels = 40;
    public const float FMin = 20f;
    public const float FMax = 8000f;
    public const int NumFrames = 1 + (ClipSamples - FrameLength) / FrameStep; 

    private static float[] hannWindow;
    private static float[,] melFilterbank; 

    static MelSpectrogram()
    {
        BuildHannWindow();
        BuildMelFilterbank();
    }

    
    
    
    public static float[] ComputeLogMel(float[] samples)
    {
        if (samples.Length != ClipSamples)
            throw new ArgumentException($"Expected exactly {ClipSamples} samples, got {samples.Length}");

        int nBins = NFFT / 2 + 1;
        float[] result = new float[NumFrames * NMels];

        float[] re = new float[NFFT];
        float[] im = new float[NFFT];
        float[] power = new float[nBins];

        for (int i = 0; i < NumFrames; i++)
        {
            int start = i * FrameStep;

            Array.Clear(re, 0, NFFT);
            Array.Clear(im, 0, NFFT);

            for (int n = 0; n < FrameLength; n++)
                re[n] = samples[start + n] * hannWindow[n];

            FFT(re, im);

            for (int k = 0; k < nBins; k++)
                power[k] = (re[k] * re[k] + im[k] * im[k]) / NFFT;

            for (int m = 0; m < NMels; m++)
            {
                float melEnergy = 0f;
                for (int k = 0; k < nBins; k++)
                    melEnergy += melFilterbank[m, k] * power[k];

                result[i * NMels + m] = Mathf_Log(melEnergy + 1e-6f);
            }
        }

        return result;
    }

    private static float Mathf_Log(float v) => (float)Math.Log(v);

    private static void BuildHannWindow()
    {
        hannWindow = new float[FrameLength];
        for (int n = 0; n < FrameLength; n++)
            hannWindow[n] = 0.5f - 0.5f * (float)Math.Cos(2.0 * Math.PI * n / (FrameLength - 1));
    }

    private static float HzToMel(float f) => 2595f * (float)Math.Log10(1.0 + f / 700.0);
    private static float MelToHz(float m) => 700f * ((float)Math.Pow(10.0, m / 2595.0) - 1f);

    private static void BuildMelFilterbank()
    {
        int nBins = NFFT / 2 + 1;
        melFilterbank = new float[NMels, nBins];

        float melMin = HzToMel(FMin);
        float melMax = HzToMel(FMax);

        float[] melPoints = new float[NMels + 2];
        for (int i = 0; i < melPoints.Length; i++)
            melPoints[i] = melMin + (melMax - melMin) * i / (NMels + 1);

        int[] binPoints = new int[NMels + 2];
        for (int i = 0; i < melPoints.Length; i++)
        {
            float hz = MelToHz(melPoints[i]);
            binPoints[i] = (int)Math.Floor((NFFT + 1) * hz / SampleRate);
        }

        for (int m = 1; m <= NMels; m++)
        {
            int fLeft = binPoints[m - 1];
            int fCenter = binPoints[m];
            int fRight = binPoints[m + 1];

            if (fCenter > fLeft)
            {
                for (int k = fLeft; k < fCenter; k++)
                    if (k >= 0 && k < nBins)
                        melFilterbank[m - 1, k] = (float)(k - fLeft) / (fCenter - fLeft);
            }
            if (fRight > fCenter)
            {
                for (int k = fCenter; k < fRight; k++)
                    if (k >= 0 && k < nBins)
                        melFilterbank[m - 1, k] = (float)(fRight - k) / (fRight - fCenter);
            }
        }
    }

    
    
    
    private static void FFT(float[] re, float[] im)
    {
        int n = re.Length;
        int bits = (int)Math.Log(n, 2);

        for (int j = 1; j < n; j++)
        {
            int rev = ReverseBits(j, bits);
            if (rev > j)
            {
                (re[j], re[rev]) = (re[rev], re[j]);
                (im[j], im[rev]) = (im[rev], im[j]);
            }
        }

        for (int len = 2; len <= n; len <<= 1)
        {
            double ang = -2.0 * Math.PI / len;
            float wRe = (float)Math.Cos(ang);
            float wIm = (float)Math.Sin(ang);

            for (int i = 0; i < n; i += len)
            {
                float curRe = 1f, curIm = 0f;
                int half = len / 2;
                for (int j = 0; j < half; j++)
                {
                    int evenIdx = i + j;
                    int oddIdx = i + j + half;

                    float evenRe = re[evenIdx], evenIm = im[evenIdx];
                    float oddRe = re[oddIdx] * curRe - im[oddIdx] * curIm;
                    float oddIm = re[oddIdx] * curIm + im[oddIdx] * curRe;

                    re[evenIdx] = evenRe + oddRe;
                    im[evenIdx] = evenIm + oddIm;
                    re[oddIdx] = evenRe - oddRe;
                    im[oddIdx] = evenIm - oddIm;

                    float nextRe = curRe * wRe - curIm * wIm;
                    float nextIm = curRe * wIm + curIm * wRe;
                    curRe = nextRe;
                    curIm = nextIm;
                }
            }
        }
    }

    private static int ReverseBits(int v, int bits)
    {
        int r = 0;
        for (int i = 0; i < bits; i++)
        {
            r = (r << 1) | (v & 1);
            v >>= 1;
        }
        return r;
    }
}
