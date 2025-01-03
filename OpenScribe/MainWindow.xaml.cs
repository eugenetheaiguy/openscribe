using System.Text;
using System.Windows;
using RestSharp;
using Newtonsoft.Json.Linq;
using System.Windows.Controls;
using System.IO;
using System.Diagnostics;
using NAudio.Wave.Compression;
using NAudio.Wave;
using System.Timers;
namespace OpenScribe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WaveInEvent waveIn;
        private WaveFileWriter waveFile;
        private System.Timers.Timer timer;
        private TimeSpan elapsedTime;
        // Supported audio and video file extensions
        private readonly string[] _supportedExtensions = { ".mp3", ".wav", ".flac", ".mp4", ".avi", ".mkv", ".mov", ".aac" };
        const string iniFilePath = @"config.ini"; // Adjust to your actual ini file location
        const string faribaIn = $@"
Whisper Transcript:

Psychiatric consultation on Fariba Arbabi. Fariba is F-A-R-I-B-A, Arbabi, A-R-B-A-B-I. Consultation is 30th of September, 2024. 
Fariba Arbabi is a 64-year-old Iranian-Canadian lady who was assessed to Wellmerica Clinic on 30th of September, 2024. 
She presented with a long-standing history of persistent sadness, lowering of mood, coma, anhedonia, reduced energy, poor motivation, 
and occasional anxiety with overthinking, overanalyzing, worrying, quite excessive, feeling shaky, nervous, restless. 
She also has a tendency to clean. She has a fear of contamination. She doesn't use public washrooms. She informed me she is now spending 
hours trying to do ritualistic activities. She also has a gambling, online gambling addiction. She has started attending Gambling Anonymous 
meeting, which has been somewhat helpful for her.

Next, any past medical history on Fariba Arbabi:
1. Tuberular adenoma.
2. Postmenopausal.
3. Sleep apnea.
4. H. pylori gastritis.
5. Colon polyps.
6. Diabetes mellitus.
7. Thyroid nodules.
8. Paroxysmal atrial fibrillation.

Next, any past psychiatric history. 
She has been treated for depression and anxiety and OCD tendencies with Paxil 10mg daily. She informed me that she hasn't tried any other antidepressants.

Next, any family history. 
Her mother is alive. She has got dementia. Her father is deceased. She has two sisters and one brother.

Next, any personal history. 
She was born and grew up in Iran. She was born and grew up in Iran. She has gone through a lot of emotional and physical abuse directed from her father. 
She went to school and finished grade 10. When she went to university, the civil war and revolution started in Iran. The university was closed for 3 years. 
However, when she moved to Canada, she trained to become an educational assistant. She has been married for 48 years. There were difficulties in their relationship. 
More recently, they initially separated and then they came back. They are living together. She has got 3 adult children.

Next, any sub-summative history. 
She had a history of gambling addiction and many online games. Now, she is attending the Gambling Anonymous.

Next, any forensic history. 
Next, any menace examination. 
Fariba presents as a middle-aged lady who was anxious during her sex.

Next, any assessment. 
Mood was subjective. Objective, she was anxious and depressed. Her form was normal. There have been 10 short symptoms. 
Consistent with major depression. With anxiety, coma, OCD tendencies. She denied any paranoia, coma, persecution, coma, or residual hallucinations.

Next, any diagnosis. 
1. Major depression with anxiety.
2. OCD tendencies.
3. Gambling addiction.

Next, any management plan. 
1. Her discussion with Fariba al-Babi about either switching the medication or optimizing the dose of paroxetine because of her sub-therapeutic doses. 
   She opted to increase the dose of paroxetine to 20 mg daily and the dose could be tightened up based on her tolerability. 
2. If she is still having problems with the Paxil and it's not effective, then I would switch it to either fluoxetine or clomipramine. 
3. I provided extensive cognitive behavioral therapy along with psychoeducation. 
4. I will review her again in 6 weeks' time.

Final Note:

Consult

Date: September 30, 2024

RE: Fariba Arbabi

HISTORY OF PRESENT ILLNESS

Fariba Arbabi is a 64-year-old, Iranian-Canadian lady who was assessed through Wellmerica Clinic on September 30, 2024. 
She presented with a longstanding history of persistent sadness, lowering of mood, anhedonia, reduced energy, and poor motivation and occasional anxiety with 
overthinking, overanalyzing, worrying quite excessively, and feeling shaky, nervous, and restless. 
She also has a tendency to clean. She has a fear of contamination and she does not use public washrooms. 
She informed me that she is now spending hours trying to do ritualistic activities. She also has an online gambling addiction. 
She started attending Gambling Anonymous meetings which have been somewhat helpful.

PAST MEDICAL HISTORY

1. Fibromyalgia.
2. Tubular adenoma.
3. Post-menopausal.
4. Sleep apnea.
5. H pylori gastritis.
6. Colon polyps.
7. Diabetes mellitus.
8. Thyroid nodules.
9. Paroxysmal atrial fibrillation.

PAST PSYCHIATRIC HISTORY

She has been treated for depression and anxiety and OCD tendencies with Paxil 10 mg daily. She informed me that she has not tried any other antidepressant.

FAMILY HISTORY

Mother is alive and has dementia. Father is deceased. She has two sisters and one brother.

PERSONAL HISTORY

She was born and grew up in Iran. Birth and early development were uneventful. She went through a lot of emotional and physical abuse directed from her father. 
She went to school and finished grade 10 and eventually she went university. When the civil war and revolution started in Iran, the university closed for three years. 
However, when she moved to Canada, she trained to become an educational assistant. She has been married for 48 years. 
There were difficulties in their relationship and they initially separated and then they came back and they are living together. She has three adult children.

SUBSTANCE MISUSE HISTORY

She had a history of a gambling addiction, mainly online games, and now she is attending the Gambling Anonymous.

FORENSIC HISTORY

Nil of note.

MENTAL STATUS EXAMINATION

Fariba presents as a middle-aged lady who was anxious during the assessment. Mood was subjectively low. Objectively, she appeared anxious and depressed. 
Thought form was normal. Thought content showed symptoms consistent with major depression with anxiety and OCD tendencies. 
She denied any paranoia, persecution, or auditory or visual hallucinations.

DIAGNOSES

1. Major depression with anxiety.
2. OCD tendencies.
3. Gambling addiction.

MANAGEMENT PLAN

1. I had a discussion with Fariba Arbabi about either switching the medication or optimizing the dose of paroxetine because it is subtherapeutic doses. 
   She opted to increase the dose of paroxetine to 20 mg daily and the dose could be titrated up based on tolerability.

2. If she is still having problems with the Paxil and it is not effective, then I would switch it to either Fluoxetine or clomipramine at the next appointment.

3. I provided extensive cognitive behavioural therapy along with psychoeducation.

4. I will review her again in six weeks' time.

**DICTATED BUT NOT READ**

kb";
        const string faribaOut = $@"
{{
  ""updated_transcript"": ""Psychiatric consultation on Fariba Arbabi. Fariba is F-A-R-I-B-A, Arbabi, A-R-B-A-B-I. Consultation is on the 30th of September, 2024. Fariba Arbabi is a 64-year-old Iranian-Canadian lady who was assessed at Well Medica Clinic on the 30th of September, 2024. She presented with a longstanding history of persistent sadness, lowering of mood, anhedonia, reduced energy, poor motivation, and occasional anxiety with overthinking, overanalyzing, worrying quite excessively, and feeling shaky, nervous, and restless. She also has a tendency to clean. She has a fear of contamination and does not use public washrooms. She informed me she is now spending hours doing ritualistic activities. She also has an online gambling addiction. She has started attending Gambling Anonymous meetings, which have been somewhat helpful for her.\n\nNext, past medical history. 1. Fibromyalgia. 2. Tubular adenoma. 3. Postmenopausal. 4. Sleep apnea. 5. H. pylori gastritis. 6. Colon polyps. 7. Diabetes mellitus. 8. Thyroid nodules. 9. Paroxysmal atrial fibrillation.\n\nNext, past psychiatric history. She has been treated for depression, anxiety, and OCD tendencies with paroxetine 10 mg daily. She informed me that she hasn't tried any other antidepressants.\n\nNext, family history. Her mother is alive and has dementia. Her father is deceased. She has two sisters and one brother.\n\nNext, personal history. She was born and grew up in Iran. Birth and early development were uneventful. She went through a lot of emotional and physical abuse directed from her father. She went to school and finished grade 10. When she went to university, the civil war and revolution started in Iran, leading to the university being closed for three years. However, when she moved to Canada, she trained to become an educational assistant. She has been married for 48 years. There were difficulties in their relationship; they initially separated but later reconciled and are now living together. She has three adult children.\n\nNext, substance misuse history. She had a history of gambling addiction, mainly online games, and now she is attending Gambling Anonymous.\n\nNext, forensic history. Nil of note.\n\nNext, mental status examination. Fariba presents as a middle-aged lady who was anxious during the assessment. Mood was subjectively low. Objectively, she appeared anxious and depressed. Thought form was normal. Thought content showed symptoms consistent with major depression with anxiety and OCD tendencies. She denied any paranoia, persecution, or auditory or visual hallucinations.\n\nNext, diagnoses. 1. Major depression with anxiety. 2. OCD tendencies. 3. Gambling addiction.\n\nNext, management plan. 1. I had a discussion with Fariba Arbabi about either switching the medication or optimizing the dose of paroxetine because it is currently subtherapeutic. She opted to increase the dose of paroxetine to 20 mg daily, which could be titrated up based on tolerability. 2. If she is still having problems with paroxetine and it is not effective, then I would switch it to either fluoxetine or clomipramine at the next appointment. 3. I provided extensive cognitive behavioural therapy along with psychoeducation. 4. I will review her again in six weeks' time."",
  ""final_note"": ""Consult\n\nDate: September 30, 2024\n\nRE: Fariba Arbabi\n\nHISTORY OF PRESENT ILLNESS\n\nFariba Arbabi is a 64-year-old, Iranian-Canadian lady who was assessed through Well Medica Clinic on September 30, 2024.  She presented with a longstanding history of persistent sadness, lowering of mood, anhedonia, reduced energy, and poor motivation and occasional anxiety with overthinking, overanalyzing, worrying quite excessively, and feeling shaky, nervous, and restless.  She also has a tendency to clean.  She has a fear of contamination and she does not use public washrooms.  She informed me that she is now spending hours trying to do ritualistic activities.  She also has an online gambling addiction.  She started attending Gambling Anonymous meetings which have been somewhat helpful.\n\nPAST MEDICAL HISTORY\n\n1. Fibromyalgia.\n2. Tubular adenoma.\n3. Post-menopausal.\n4. Sleep apnea.\n5. H pylori gastritis.\n6. Colon polyps.\n7. Diabetes mellitus.\n8. Thyroid nodules.\n9. Paroxysmal atrial fibrillation.\n\nPAST PSYCHIATRIC HISTORY\n\nShe has been treated for depression and anxiety and OCD tendencies with Paxil 10 mg daily.  She informed me that she has not tried any other antidepressant.\n\nFAMILY HISTORY\n\nMother is alive and has dementia.  Father is deceased.  She has two sisters and one brother.\n\nPERSONAL HISTORY\n\nShe was born and grew up in Iran.  Birth and early development were uneventful.  She went through a lot of emotional and physical abuse directed from her father.  She went to school and finished grade 10 and eventually she went university.  When the civil war and revolution started in Iran, the university closed for three years.  However, when she moved to Canada, she trained to become an educational assistant.  She has been married for 48 years.  There were difficulties in their relationship and they initially separated and then they came back and they are living together.  She has three adult children.\n\nSUBSTANCE MISUSE HISTORY\n\nShe had a history of a gambling addiction, mainly online games, and now she is attending the Gambling Anonymous.\n\nFORENSIC HISTORY\n\nNil of note.\n\nMENTAL STATUS EXAMINATION\n\nFariba presents as a middle-aged lady who was anxious during the assessment.  Mood was subjectively low.  Objectively, he appeared anxious and depressed.  Thought form was normal.  Thought content showed symptoms consistent with major depression with anxiety and OCD tendencies.  She denied any paranoia, persecution, or auditory or visual hallucinations.\n\nDIAGNOSES\n\n1. Major depression with anxiety.\n2. OCD tendencies.\n3. Gambling addiction.\n\nMANAGEMENT PLAN\n\n1. I had a discussion with Fariba Arbabi about either switching the medication or optimizing the dose of paroxetine because it is subtherapeutic doses.  She opted to increase the dose of paroxetine to 20 mg daily and the dose could be titrated up based on tolerability.\n\n2. If she is still having problems with the Paxil and it is not effective, then I would switch it to either Fluoxetine or clomipramine at the next appointment.\n\n3. I provided extensive cognitive behavioural therapy along with psychoeducation.\n\n4. I will review her again in six weeks' time.\n\n**DICTATED BUT NOT READ**\n\nkb"",
  ""change_summary"": [
    {{
      ""before"": ""assessed to Wellmerica Clinic"",
      ""after"": ""assessed at Well Medica Clinic"",
      ""description"": ""Corrected the clinic name to match the final note.""
    }},
    {{
      ""before"": ""persistent sadness, lowering of mood, coma, anhedonia"",
      ""after"": ""persistent sadness, lowering of mood, anhedonia"",
      ""description"": ""Removed 'coma,' which was a clear transcription error.""
    }},
    {{
      ""before"": ""feeling shaky, nervous, restless. She also has a tendency to clean."",
      ""after"": ""feeling shaky, nervous, and restless. She also has a tendency to clean."",
      ""description"": ""Improved sentence flow for clarity and grammar.""
    }},
    {{
      ""before"": ""gambling, online gambling addiction"",
      ""after"": ""online gambling addiction"",
      ""description"": ""Consolidated repetitive phrasing.""
    }},
    {{
      ""before"": ""tuberular adenoma"",
      ""after"": ""tubular adenoma"",
      ""description"": ""Corrected medical terminology.""
    }},
    {{
      ""before"": ""she hasn't tried any other antidepressants."",
      ""after"": ""she has not tried any other antidepressants."",
      ""description"": ""Adjusted phrasing for consistency with clinical tone.""
    }},
    {{
      ""before"": ""Fariba presents as a middle-aged lady who was anxious during her sex."",
      ""after"": ""Fariba presents as a middle-aged lady who was anxious during the assessment."",
      ""description"": ""Corrected transcription error 'her sex' to 'the assessment.'""
    }}
  ]
}}";
        const string meganIn = $@"
Whisper Transcript:

I get to consider Megan Teixeira. Megan is M-E-G-A-N T-E-I-X-E-I-R-A, consideration date 30th September 2024. Megan Teixeira is a 33-year-old lady who was assessed at Alameda Cardiac Clinic on 30th September 2024. She was referred to me with a history of ADHD as well as some mixed anxiety and depression. She is currently maintained on Vyvanse 30 mg daily, which works in the morning. However, she feels the effects wear off in the afternoon. She is also taking Wellbutrin 100 mg p.o. b.i.d. without any negative effects.

Next scene is personal history. She was born and grew up in Toronto, Ontario. Birth and early development were uneventful, and she denied any birth-related injuries or early mental health issues. She went to school, finished grade 12, has some college, works as a bookkeeper, and is single.

Next scene is a medical examination. Megan presents as a young lady who was pleasant and cooperative during the assessment. Speech was spontaneous and coherent. Mood was subjectively good. Objectively, she appeared mildly anxious. Thought form was normal. Thought content showed symptoms consistent with ADHD, inattentive type, and generalized anxiety disorder. She denied any paranoia, persecution, or visual hallucinations.

Next scene is a diagnosis:
1. ADHD, inattentive type.
2. Mixed anxiety and depression.

Next scene is a management plan:
1. Megan is maintained on Vyvanse, and we agreed to optimize the dose to 30 mg in the morning and 10 mg at noon.
2. She will continue taking Wellbutrin 100 mg p.o. b.i.d.
3. I provided cognitive behavioral therapy along with psychoeducation.
4. I will review her again in two months' time.

Final Note:

Consult

Date: September 30, 2024

RE: Megan Teixeira

HISTORY OF PRESENT ILLNESS

Megan Teixeira is a 33-year-old lady who was assessed at Alameda Cardiac Clinic on September 30, 2024. She was referred to me with a history of ADHD as well as some mixed anxiety and depression. She is currently maintained on Vyvanse 30 mg daily, which works in the morning. However, she feels the effects wear off in the afternoon. She is also taking Wellbutrin 100 mg p.o. b.i.d. without any negative effects.

PERSONAL HISTORY

She was born and grew up in Toronto, Ontario. Birth and early development were uneventful. She denied any birth-related injuries or early traumatic life experiences. She went to school and finished grade 12. She has some college and works as a bookkeeper and is single.

MENTAL STATUS EXAMINATION

Megan presents as a young lady who was pleasant and cooperative during the assessment. Speech was spontaneous and coherent. Mood was subjectively good. Objectively, she appeared mildly anxious. Thought form was normal. Thought content showed symptoms consistent with ADHD, inattentive type, and generalized anxiety disorder. She denied any paranoia, persecution, or auditory or visual hallucinations.

DIAGNOSES

1. ADHD, inattentive type.
2. Mixed anxiety and depression.

MANAGEMENT PLAN

1. Megan is maintained on Vyvanse, and we agreed to optimize the dose to 30 mg in the morning and 10 mg at noon.
2. I will continue with Wellbutrin 100 mg p.o. b.i.d.
3. I provided cognitive behavioral therapy along with psychoeducation.
4. I will review her in two months' time.

**DICTATED BUT NOT READ**

kb";
        const string meganOut = $@"
{{
  ""updated_transcript"": ""Psychiatric consultation on Megan Teixeira. Megan is M-E-G-A-N, Teixeira, T-E-I-X-E-I-R-A, consultation date: 30th September 2024. Megan Teixeira is a 33-year-old lady who was assessed at Well Medica Clinic on 30th September 2024. She was referred to me with a history of ADHD as well as some mixed anxiety and depression. She is currently maintained on Vyvanse 30 mg daily, which does work in the morning. However, she feels the effects wear off in the afternoon. She also is taking Wellbutrin 100 mg p.o. b.i.d without any negative effects. Next is personal history. She was born and grew up in Toronto, Ontario. Birth and early development were uneventful, for which she denied any birth related injuries or early traumatic life experiences. She went to school, finished grade 12. She has some college and works as a bookkeeper. She is single. Next is mental status examination. Megan presents as a young lady who was pleasant and cooperative during the assessment. Speech was spontaneous and coherent. Mood was subjectively good. Objectively, she appeared mildly anxious. Thought form was normal, thought content showed symptoms consistent with ADHD inattentive type, generalized anxiety disorder. She denied any paranoia, persecution, or auditory or visual hallucinations. Next is a diagnosis. One is ADHD inattentive type. Number two is mixed anxiety and depression. Next is a management plan. Megan is maintained on Vyvanse and we agreed to optimize the dose to 30 mg in the morning and 10 mg at noon. We will continue with the Wellbutrin 100 mg p.o. b.i.d. Number three provided cognitive behavioural therapy and psychoeducation."",
  ""final_note"": ""Consult\n\nDate: September 30, 2024\n\nRE: Megan Teixeira\n\nHISTORY OF PRESENT ILLNESS\n\nMegan Teixeira is a 33-year-old lady who was assessed through Well Medica Clinic on September 30, 2024. She was referred to me with a history of ADHD as well as some mixed anxiety and depression. She is currently maintained on Vyvanse 30 mg daily, which does work in the morning. However, she feels the effects wear off in the afternoon. She is also taking Wellbutrin 100 mg p.o. b.i.d. without any negative effects.\n\nPERSONAL HISTORY\n\nShe was born and grew up in Toronto, Ontario. Birth and early development were uneventful. She denied any birth-related injuries or early traumatic life experiences. She went to school and finished grade 12. She has some college and works as a bookkeeper and is single.\n\nMENTAL STATUS EXAMINATION\n\nMegan presents as a young lady who was pleasant and cooperative during the assessment. Speech was spontaneous and coherent. Mood was subjectively good. Objectively, she appeared mildly anxious. Thought form was normal. Thought content showed symptoms consistent with ADHD, inattentive type, and generalized anxiety disorder. She denied any paranoia, persecution, or auditory or visual hallucinations.\n\nDIAGNOSES\n\n1. ADHD, inattentive type.\n2. Mixed anxiety and depression.\n\nMANAGEMENT PLAN\n\n1. Megan is maintained on Vyvanse and we agreed to optimize the dose to 30 mg in the morning and 10 mg at noon.\n\n2. I will continue with the Wellbutrin 100 mg p.o. b.i.d.\n\n3. I provided cognitive behavioural therapy along with psychoeducation.\n\n4. I will review her in two months' time.\n\n**DICTATED BUT NOT READ**\n\nkb"",
  ""change_summary"": [
    {{
      ""before"": ""I get to consider Megan Tixera."",
      ""after"": ""Psychiatric consultation on Megan Teixeira."",
      ""description"": ""Corrected the introductory phrase and spelling of patient's surname.""
    }},
    {{
      ""before"": ""Y1's 30 mg daily"",
      ""after"": ""Vyvanse 30 mg daily"",
      ""description"": ""Corrected the medication name.""
    }},
    {{
      ""before"": ""Welbuton 100 mg POBID"",
      ""after"": ""Wellbutrin 100 mg p.o. b.i.d"",
      ""description"": ""Corrected the medication name and dosage instruction.""
    }},
    {{
      ""before"": ""puts her birth and undergrowth and was the only one"",
      ""after"": ""Birth and early development were uneventful,"",
      ""description"": ""Corrected the transcription error.""
    }},
    {{
      ""before"": ""Next scene is a medical examination."",
      ""after"": ""Next is mental status examination."",
      ""description"": ""Corrected the phrasing to match final note.""
    }},
    {{
      ""before"": ""Next scene is a diagnosis."",
      ""after"": ""Next is a diagnosis."",
      ""description"": ""Corrected the phrasing to match final note.""
    }},
    {{
      ""before"": ""The meds will contain Welbuton 100 mg"",
      ""after"": ""We will continue with the Wellbutrin 100 mg p.o. b.i.d."",
      ""description"": ""Corrected the medication name and phrasing to match final note.""
    }}
  ]
}}
";
        const string arienIn = $@"
Whisper Transcript:

So I get a consultation on Arian Bryson Russell. Arian is A-R-I-E-N Bryson B-R-Y-S-E-N Russell R-U-S-S-E-L-L. Consultation on 30th September 2024. Arian Bryson Russell is a 37-year-old gentleman who was assessed at World Medical Clinic on 30th September 2024. He has been known to have a history of ADHD. He was given a trial of Adderall 10 mg daily which has been somewhat effective. However, he would like for it to be optimized to get the maximum benefit. He did not experience any side effects. He denied any ideas of hopelessness, worthlessness, death wishes, or suicidal ideation.

Next, personal history. He was born and grew up in Toronto, Ontario. Birth and early development were uneventful. He went to school, finished grade 5. He has been to university but dropped out. Currently, he is trying to become a personal trainer. He has been in a relationship for two and a half years.

Next, medical examination. Arian, A-R-I-E-N, presents as a young gentleman who was pleasant and cooperative during the assessment. Speech was spontaneous and coherent. Mood was subjectively good. Objectively, he appeared mildly anxious. Thought form was normal. Thought content showed symptoms consistent with ADHD, inattentive type. He denied any paranoia, persecution, or hallucinations.

Diagnosis:
- ADHD, inattentive type.

Management Plan:
1. Arian is maintained on Adderall 10 mg daily. We agreed to optimize the dose to 20 mg to get the maximum benefit.
2. I provided cognitive behavioural therapy along with psychoeducation.
3. I will review him in two to three months' time.

Final Note:

Consult

Date: September 30, 2024

RE: Arien Brysen Russell

HISTORY OF PRESENT ILLNESS

Arien Brysen Russell is a 37-year-old gentleman who was assessed through Well Medica Clinic on September 30, 2024. He has been known to me with a history of ADHD. He was given a trial of Adderall 10 mg daily which has been somewhat effective. However, he would like the dose optimized to get the maximum benefit. He denied experiencing any side effects. He denied any ideas of hopelessness, worthlessness, death wish, or suicidal ideation.

PERSONAL HISTORY

He was born and grew up in Toronto, Ontario. Birth and early development were uneventful. He went to school and finished grade 12. He has been to university but dropped out. He is currently trying to become a personal trainer. He has been in a relationship for two and a half years.

MENTAL STATUS EXAMINATION

Arien presents as a young gentleman who was pleasant and cooperative during the assessment. Speech was spontaneous and coherent. Mood was subjectively good. Objectively, he appeared mildly anxious. Thought form was normal. Thought content showed symptoms consistent with ADHD, inattentive type. He denied any paranoia, persecution, or auditory or visual hallucinations.

DIAGNOSIS

ADHD, inattentive type.

MANAGEMENT PLAN

1. Arien is maintained on Adderall 10 mg daily and we agreed to optimize the dose to 20 mg to get the maximum benefit.
2. I provided him with cognitive behavioural therapy along with psychoeducation.
3. I will review him in two to three months' time.

**DICTATED BUT NOT READ**

kb";
        const string arienOut = $@"
{{
  ""updated_transcript"": ""Psychiatric consultation on Arien Brysen Russell. Arien is A-R-I-E-N, Brysen, B-R-Y-S-E-N, Russell, R-U-S-S-E-L-L. Consultation date is the 30th of September, 2024. Arien Brysen Russell is a 37-year-old gentleman who was assessed at Well Medica Clinic on the 30th of September, 2024. He has been known to have a history of ADHD. He was given a trial of Adderall 10 mg daily, which has been somewhat effective. However, he would like the dose to be optimized to get the maximum benefit. He denied experiencing any side effects. He also denied any ideas of hopelessness, worthlessness, death wish, or suicidal ideation.\n\nNext, personal history. He was born and grew up in Toronto, Ontario. Birth and early development were uneventful. He went to school and finished grade 12. He has been to university but dropped out. He is currently trying to become a personal trainer. He has been in a relationship for two and a half years.\n\nNext, mental status examination. Arien presents as a young gentleman who was pleasant and cooperative during the assessment. Speech was spontaneous and coherent. Mood was subjectively good. Objectively, he appeared mildly anxious. Thought form was normal. Thought content showed symptoms consistent with ADHD, inattentive type. He denied any paranoia, persecution, or auditory or visual hallucinations.\n\nNext, diagnosis. ADHD, inattentive type.\n\nNext, management plan. 1. Arien is maintained on Adderall 10 mg daily, and we agreed to optimize the dose to 20 mg to get the maximum benefit. 2. I provided him with cognitive behavioural therapy along with psychoeducation. 3. I will review him in two to three months' time."",
  ""final_note"": ""Consult\n\nDate: September 30, 2024\n\nRE: Arien Brysen Russell\n\nHISTORY OF PRESENT ILLNESS\n\nArien Brysen Russell is a 37-year-old gentleman who was assessed through Well Medica Clinic on September 30, 2024.  He has been known to me with a history of ADHD.  He was given a trial of Adderall 10 mg daily which has been somewhat effective.  However, he would like the dose optimized to get the maximum benefit.  He denied experiencing any side effects.  He denied any ideas of hopelessness, worthlessness, death wish, or suicidal ideation.\n\nPERSONAL HISTORY\n\nHe was born and grew up in Toronto, Ontario.  Birth and early development were uneventful.  He went to school and finished grade 12.  He has been to university but dropped out.  He is currently trying to become a personal trainer.  He has been in a relationship for two and a half years.\n\nMENTAL STATUS EXAMINATION\n\nArien presents as a young gentleman who was pleasant and cooperative during the assessment.  Speech was spontaneous and coherent.  Mood was subjectively good.  Objectively, he appeared mildly anxious.  Thought form was normal.  Thought content showed symptoms consistent with ADHD, inattentive type.  He denied any paranoia, persecution, or auditory or visual hallucinations.\n\nDIAGNOSIS\n\nADHD, inattentive type.\n\nMANAGEMENT PLAN\n\n1. Arien is maintained on Adderall 10 mg daily and we agreed to optimize the dose to 20 mg to get the maximum benefit.\n\n2. I provided him with cognitive behavioural therapy along with psychoeducation.\n\n3. I will review him in two to three months' time.\n\n**DICTATED BUT NOT READ**\n\nkb"",
  ""change_summary"": [
    {{
      ""before"": ""assessed to World Medical Clinic"",
      ""after"": ""assessed at Well Medica Clinic"",
      ""description"": ""Corrected the clinic name and phrasing to match the final note.""
    }},
    {{
      ""before"": ""Adderall 10 mgm daily"",
      ""after"": ""Adderall 10 mg daily"",
      ""description"": ""Fixed dosage format to align with clinical documentation standards.""
    }},
    {{
      ""before"": ""denied any idea of hopelessness, worthlessness, death, vicious, suicidal ideation."",
      ""after"": ""denied any ideas of hopelessness, worthlessness, death wish, or suicidal ideation."",
      ""description"": ""Corrected phrasing for clarity and clinical accuracy.""
    }},
    {{
      ""before"": ""finished grade 5"",
      ""after"": ""finished grade 12"",
      ""description"": ""Corrected educational history based on the final note.""
    }},
    {{
      ""before"": ""pleasantly incorporated during the assessment speech. He was spontaneous and go ahead and move to subject record."",
      ""after"": ""pleasant and cooperative during the assessment. Speech was spontaneous and coherent."",
      ""description"": ""Rephrased and corrected nonsensical transcription errors for coherence.""
    }},
    {{
      ""before"": ""mildly anxious thought form. There was no other content to it."",
      ""after"": ""mildly anxious. Thought form was normal. Thought content showed symptoms consistent with ADHD, inattentive type."",
      ""description"": ""Clarified thought form and content to match clinical findings.""
    }},
    {{
      ""before"": ""giving him 2-3 months time to receive a doctor's appointment."",
      ""after"": ""review him in two to three months' time."",
      ""description"": ""Adjusted phrasing for clarity and to reflect the final note accurately.""
    }}
  ]
}}";
        const string emelinaIn = $@"
Whisper Transcript:

So, I got a consultation on Emelina Roman. 

Emelina is E-M-E-L-I-N-A, Roman, R-O-M-A-N. Consultation is 30th of September, 2024. Emelina Roman is a 55-year-old lady who was assessed at Well Medica Clinic on the 30th of September, 2024. She has been known to have a history of major depression and anxiety. She informed me that the sertraline has been working okay, but she has not seen a huge benefit. However, she would like the dose optimized to get the maximum benefit. She had difficulty tolerating Seroquel; it gave her some nightmares; therefore, she decided to go off as it did not make any difference. However, she is willing to go back on the trazodone 50 mg at h.s.

Personal history. She was born and grew up in Nicaragua. Nicaragua is spelled N-I-C-A-R-A-G-U-A. Birth and early development were uneventful. She grew up in an era when there was a civil war. She witnessed a lot of trauma, and people were killed in front of her eyes when she was only 10 years of age. She went to school and finished grade 12. She trained to become a PSW caregiver, and she has been able to maintain a regular job for the last two years. She is single. She was married for 25 years and got divorced. There was a lot of abuse in the marriage. She has three children.

Mental status examination. Emelina presents as a middle-aged lady who was anxious during the assessment. Mood was subjectively low. Objectively, she appeared anxious and depressed. Thought form was normal. Thought content showed symptoms consistent with major depressive disorder, anxiety disorder, and PTSD-like symptoms. She denied any paranoia, persecution, or auditory or visual hallucinations.

Diagnosis:
1. Major depressive disorder.
2. Generalized anxiety disorder.
3. PTSD-like symptoms.

Management Plan:
1. Emelina is maintained on sertraline, and we agreed to optimize the dose to 75 mg daily for a week followed by an increase to 100 mg daily.
2. I will discontinue Seroquel and replace it with trazodone 50 mg at h.s.
3. I have also given her some prazosin to help her with nightmares and bad dreams.
4. I provided her with cognitive behavioural therapy along with psychoeducation.
5. I will review her in two months' time.

Final Note:

Consult

Date: September 30, 2024

RE: Emelina Roman

HISTORY OF PRESENT ILLNESS

Emelina Roman is a 55-year-old lady who was assessed through Well Medica Clinic on September 30, 2024. She has been known to me with a history of major depression and anxiety. She informed me that the sertraline has been working okay, but she has not seen a huge benefit. However, she would like the dose optimized to get the maximum benefit. She had difficulty tolerating Seroquel; it gave her some nightmares; therefore, she decided to go off as it did not make any difference. However, she is willing to go back on the Trazodone 50 mg at h.s.

PERSONAL HISTORY

She was born and grew up in Nicaragua. Birth and early development were uneventful. She grew up in an era when there was a civil war. She witnessed a lot of trauma, and people were killed in front of her eyes when she was only 10 years of age. She went to school and finished grade 12. She trained to become a PSW caregiver, and she has been able to maintain a regular job for the last two years. She is single. She was married for 25 years and got divorced. There was a lot of abuse in the marriage. She has three children.

MENTAL STATUS EXAMINATION

Emelina presents as a middle-aged lady who was anxious during assessment. Mood was subjectively low. Objectively, she appeared anxious and depressed. Thought form was normal. Thought content showed symptoms consistent with major depressive disorder, anxiety disorder, and PTSD-like symptoms. She denied any paranoia, persecution, or auditory or visual hallucinations.

DIAGNOSES

1. Major depressive disorder.
2. Generalized anxiety disorder.
3. PTSD-like symptoms.

MANAGEMENT PLAN

1. Emelina is maintained on sertraline, and we agreed to optimize the dose to 75 mg daily for a week followed by an increase to 100 mg daily.
2. I will discontinue Seroquel and replace it with Trazodone 50 mg at h.s.
3. I have also given her some prazosin to help her with the nightmares and bad dreams.
4. I provided her with cognitive behavioural therapy along with psychoeducation.
5. I will review her in two months' time.

**DICTATED BUT NOT READ**

kb";
        const string emelinaOut = $@"
{{
  ""updated_transcript"": ""Psychiatric consultation on Emelina Roman. Emelina is E-M-E-L-I-N-A, Roman, R-O-M-A-N. Consultation date is the 30th of September, 2024. Emelina Roman is a 55-year-old lady who was assessed at Well Medica Clinic on the 30th of September, 2024. She has been known to have a history of major depression and anxiety. She informed me that the sertraline has been working okay, but she has not seen a huge benefit. However, she would like the dose optimized to get the maximum benefit. She had difficulty tolerating Seroquel as it gave her some nightmares. Therefore, she decided to stop taking it as it did not make any difference. However, she is willing to go back on Trazodone 50 mg at bedtime.\n\nNext, personal history. She was born and grew up in Nicaragua. Birth and early development were uneventful. She grew up in an era when there was a civil war. She witnessed significant trauma, including people being killed in front of her eyes when she was only 10 years old. She went to school and finished grade 12. Later, she trained to become a PSW caregiver and has been able to maintain a regular job for the last two years. She is single. She was married for 25 years and got divorced. There was significant abuse in the marriage. She has three children.\n\nNext, mental status examination. Emelina presents as a middle-aged lady who was anxious during the assessment. Mood was subjectively low. Objectively, she appeared anxious and depressed. Thought form was normal. Thought content showed symptoms consistent with major depressive disorder, anxiety disorder, and PTSD-like symptoms. She denied any paranoia, persecution, or auditory or visual hallucinations.\n\nNext, diagnoses. 1. Major depressive disorder. 2. Generalized anxiety disorder. 3. PTSD-like symptoms.\n\nNext, management plan. 1. Emelina is maintained on sertraline, and we agreed to optimize the dose to 75 mg daily for a week, followed by an increase to 100 mg daily. 2. I will discontinue Seroquel and replace it with Trazodone 50 mg at bedtime. 3. I have also given her prazosin to help with nightmares and bad dreams. 4. I provided her with cognitive behavioural therapy along with psychoeducation. 5. I will review her again in two months' time."",
  ""final_note"": ""Consult\n\nDate: September 30, 2024\n\nRE: Emelina Roman\n\nHISTORY OF PRESENT ILLNESS\n\nEmelina Roman is a 55-year-old lady who was assessed through Well Medica Clinic on September 30, 2024.  She has been known to me with a history of major depression and anxiety.  She informed me that the sertraline has been working okay, but she has not seen a huge benefit.  However, she would like the dose optimized to get the maximum benefit.  She had difficulty tolerating Seroquel; it gave her some nightmares; therefore, she decided to go off as it did not make any difference.  However, she is willing to go back on the Trazodone 50 mg at h.s. \n\nPERSONAL HISTORY\n\nShe was born and grew up in Nicaragua.  Birth and early development were uneventful.  She grew up in an era when there was a civil war.  She witnessed a lot of trauma and people were killed in front of her eyes when she was only 10 years of age.  She went to school and finished grade 12.  She trained to become a PSW caregiver and she has been able to maintain a regular job for the last two years.  She is single.  She was married for 25 years and got divorced.  There was a lot of abuse in the marriage.  She has three children.\n\nMENTAL STATUS EXAMINATION\n\nEmelina presents as a middle-aged lady who was anxious during assessment.  Mood was subjectively low.  Objectively, she appeared anxious and depressed.  Thought form was normal.  Thought content showed symptoms consistent with major depressive disorder, anxiety disorder, and PTSD-like symptoms.  She denied any paranoia, persecution, or auditory or visual hallucinations.\n\nDIAGNOSES\n\n1. Major depressive disorder.\n2. Generalized anxiety disorder.\n3. PTSD-like symptoms.\n\nMANAGEMENT PLAN\n\n1. Emelina is maintained on sertraline and we agreed to optimize the dose to 75 mg daily for a week followed by increase to 100 mg daily.\n\n2. I will discontinue Seroquel and replace it with Trazodone 50 mg at h.s. \n\n3. I have also given her some prazosin to help her with the nightmares and bad dreams.\n\n4. I provided her with cognitive behavioural therapy along with psychoeducation.\n\n5. I will review her in two months' time.\n\n**DICTATED BUT NOT READ**\n\nkb"",
  ""change_summary"": [
    {{
      ""before"": ""assessed to World Medical Clinic"",
      ""after"": ""assessed at Well Medica Clinic"",
      ""description"": ""Corrected the clinic name and phrasing to match the final note.""
    }},
    {{
      ""before"": ""the surgery had been working okay"",
      ""after"": ""the sertraline has been working okay"",
      ""description"": ""Corrected transcription error confusing 'surgery' with 'sertraline'.""
    }},
    {{
      ""before"": ""she was willing to go back on the treasure of 50mg of chestnuts."",
      ""after"": ""she is willing to go back on Trazodone 50 mg at bedtime."",
      ""description"": ""Corrected nonsensical transcription to reflect accurate medication and dosage.""
    }},
    {{
      ""before"": ""Her mood was subject to the objective."",
      ""after"": ""Mood was subjectively low. Objectively, she appeared anxious and depressed."",
      ""description"": ""Improved clarity and aligned with clinical terminology.""
    }},
    {{
      ""before"": ""shenanigans, paranoia, persecution, and moderate hallucinations."",
      ""after"": ""symptoms consistent with major depressive disorder, anxiety disorder, and PTSD-like symptoms. She denied any paranoia, persecution, or auditory or visual hallucinations."",
      ""description"": ""Corrected nonsensical phrasing and replaced it with clinically relevant findings.""
    }},
    {{
      ""before"": ""we agreed to optimize her dose to 75mg daily for a week for one increase to 100mg daily."",
      ""after"": ""we agreed to optimize the dose to 75 mg daily for a week, followed by an increase to 100 mg daily."",
      ""description"": ""Improved phrasing for clarity and grammatical accuracy.""
    }},
    {{
      ""before"": ""and replace it with a trezorone 50mg daily chest."",
      ""after"": ""and replace it with Trazodone 50 mg at bedtime."",
      ""description"": ""Fixed transcription errors in medication name and dosage instructions.""
    }}
  ]
}}";
        const string ashleyIn = $@"
Whisper Transcript:

So I get a consideration on Ashley Angamarca Rios, Ashley's A-S-H-L-E-Y, Angamarca A-N-G-A-M-A-R-C-A, Rios R-I-O-S, consideration date 30th of September, 2024. Ashley Angamarca Rios is a 25-year-old lady who was assessed at Well Medica Clinic on 30th of September, 2024. She has a history of major depression and anxiety along with ADHD. She was given a trial of Vyvanse 20 mg daily with some benefits. She would like to have the dose optimized to get the maximum benefit. She denied experiencing any side effects. She probably had the generic version which was not as effective. Therefore, she would rather use no substitute branded medication per trial.

Next is personal history. She was born and grew up in Toronto. Birth and early development were uneventful. She went through some sexual molestation when she was a child. She also had difficulty in a previous relationship with a lot of emotional abuse. She finished grade 12, went to college, and is currently working two jobs. The first is for Air Canada doing cleaning, and the second is as a skin care brand representative. She has been in a relationship for a year and describes it as stable.

Next is mental status examination. Ashley presents as a young lady who was pleasant and cooperative during the assessment. Speech was spontaneous and coherent. Mood was subjectively good. Objectively, she appeared mildly anxious. Thought form was normal. Thought content showed symptoms consistent with generalized anxiety disorder, major depressive disorder, and ADHD inattentive type. She denied any paranoia, persecution, or auditory or visual hallucinations.

Next is diagnosis:
1. Major depression with anxiety (remission).
2. ADHD, inattentive type.

Next is management plan:
1. Ashley is maintained on Vyvanse and we agreed to optimize the dose to 30 mg daily.
2. I provided some cognitive behavioural therapy along with psychoeducation.
3. I will review her again in two to three months' time.

Final Note:

Consult

Date: September 30, 2024

RE: Ashley Angamarca-Rios

HISTORY OF PRESENT ILLNESS

Ashley Angamarca-Rios is a 25-year-old lady who was assessed through Well Medica Clinic on September 30, 2024. She has been known with a history of major depression with anxiety along with ADHD. She was given a trial of Vyvanse 20 mg daily with some benefit. She would like the dose optimized to get the maximum benefit. She denied experiencing any side effects. She probably has the generic version which was not as effective; therefore, she would rather use no substitute, a branded medication.

PERSONAL HISTORY

She was born and grew up in Toronto. Birth and early development were uneventful. She went through some sexual molestation when she was a child. She also had difficulty in a previous relationship with a lot of emotional abuse. She finished grade 12 and later went to college. She is currently working at two jobs. The first one is for Air Canada doing cleaning and she is also a skin care brand representative. She has been in a relationship for a year and describes a stable relationship.

MENTAL STATUS EXAMINATION

Ashley presents as a young lady who was pleasant and cooperative during the assessment. Speech was spontaneous and coherent. Mood was subjectively good. Objectively, she appeared mildly anxious. Thought form was normal. Thought content showed symptoms consistent with generalized anxiety disorder, major depressive disorder, and ADHD inattentive type. She denied any paranoia, persecution, or auditory or visual hallucinations.

DIAGNOSES

1. Major depression with anxiety (remission).
2. ADHD, inattentive type.

MANAGEMENT PLAN

1. Ashley is maintained on Vyvanse and we agreed to optimize the dose to 30 mg daily.
2. I provided some cognitive behavioural therapy along with psychoeducation.
3. I will review her again in two to three months' time.

**DICTATED BUT NOT READ**

kb";
        const string ashleyOut = $@"
{{
  ""updated_transcript"": ""Psychiatric consultation on Ashley Angamarca-Rios. Ashley is A-S-H-L-E-Y, Angamarca, A-N-G-A-M-A-R-C-A, Rios, R-I-O-S. Consultation date is the 30th of September, 2024. Ashley Angamarca-Rios is a 25-year-old lady who was assessed at Well Medica Clinic on the 30th of September, 2024. She has a history of major depression with anxiety along with ADHD. She was given a trial of Vyvanse 20 mg daily with some benefit. She would like the dose optimized to get the maximum benefit. She denied experiencing any side effects. She likely had the generic version, which was not as effective; therefore, she would prefer to use the branded medication without substitution.\n\nNext, personal history. She was born and grew up in Toronto. Birth and early development were uneventful. She went through sexual molestation as a child. She also experienced emotional abuse in a previous relationship. She finished grade 12 and went to college. She is currently working two jobs: one for Air Canada in cleaning and the other as a skin care brand representative. She has been in a relationship for a year and describes it as stable.\n\nNext, mental status examination. Ashley presents as a young lady who was pleasant and cooperative during the assessment. Speech was spontaneous and coherent. Mood was subjectively good. Objectively, she appeared mildly anxious. Thought form was normal. Thought content showed symptoms consistent with generalized anxiety disorder, major depressive disorder, and ADHD, inattentive type. She denied any paranoia, persecution, or auditory or visual hallucinations.\n\nNext, diagnoses. 1. Major depression with anxiety (in remission). 2. ADHD, inattentive type.\n\nNext, management plan. 1. Ashley is maintained on Vyvanse, and we agreed to optimize the dose to 30 mg daily. 2. I provided her with cognitive behavioural therapy along with psychoeducation. 3. I will review her again in two to three months' time."",
  ""final_note"": ""Consult\n\nDate: September 30, 2024\n\nRE: Ashley Angamarca-Rios\n\nHISTORY OF PRESENT ILLNESS\n\nAshley Angamarca-Rios is a 25-year-old lady who was assessed through Well Medica Clinic on September 30, 2024.  She has been known with a history of major depression with anxiety along with ADHD.  She was given a trial of Vyvanse 20 mg daily with some benefit.  She would like the dose optimized to get the maximum benefit.  She denied experiencing any side effects.  She probably has the generic version which was not as effective; therefore, she would rather use no substitute, a branded medication.\n\nPERSONAL HISTORY\n\nShe was born and grew up in Toronto.  Birth and early development were uneventful.  She went through some sexual molestation when she was a child.  She also had difficulty in a previous relationship with a lot of emotional abuse.  She finished grade 12 and later went to college.  She is currently working at two jobs.  The first one is for Air Canada doing cleaning and she is also a skin care brand representative.  She has been in a relationship for a year and describes a stable relationship.\n\nMENTAL STATUS EXAMINATION\n\nAshley presents as a young lady who was pleasant and cooperative during the assessment.  Speech was spontaneous and coherent.  Mood was subjectively good.  Objectively, she appeared mildly anxious.  Thought form was normal.  Thought content showed symptoms consistent with generalized anxiety disorder, major depressive disorder, and ADHD inattentive type.  She denied any paranoia, persecution, or auditory or visual hallucinations.\n\nDIAGNOSES\n\n1. Major depression with anxiety (remission).\n2. ADHD, inattentive type.\n\nMANAGEMENT PLAN\n\n1. Ashley is maintained on Vyvanse and we agreed to optimize the dose to 30 mg daily.\n\n2. I provided some cognitive behavioural therapy along with psychoeducation.\n\n3. I will review her again in two to three months' time.\n\n**DICTATED BUT NOT READ**\n\nkb"",
  ""change_summary"": [
    {{
      ""before"": ""assessed to America Clinic"",
      ""after"": ""assessed at Well Medica Clinic"",
      ""description"": ""Corrected the clinic name and phrasing to match the final note.""
    }},
    {{
      ""before"": ""20-20mg daily"",
      ""after"": ""20 mg daily"",
      ""description"": ""Corrected the dosage format to match clinical documentation standards.""
    }},
    {{
      ""before"": ""no substitute branded medication per trial."",
      ""after"": ""the branded medication without substitution."",
      ""description"": ""Improved clarity and phrasing for readability.""
    }},
    {{
      ""before"": ""Mood was subject to record. Objective she was mildly anxious."",
      ""after"": ""Mood was subjectively good. Objectively, she appeared mildly anxious."",
      ""description"": ""Corrected unclear phrasing and aligned with clinical language.""
    }},
    {{
      ""before"": ""generalised anxiety disorder, major depression disorder, ADHD, I don't know if you want to pursue a question or come out."",
      ""after"": ""generalized anxiety disorder, major depressive disorder, and ADHD, inattentive type."",
      ""description"": ""Corrected nonsensical phrasing to match the clinical findings.""
    }},
    {{
      ""before"": ""Number two was ADHD and addictive type."",
      ""after"": ""ADHD, inattentive type."",
      ""description"": ""Fixed transcription error and improved clarity.""
    }},
    {{
      ""before"": ""We agreed to optimize the dose to 33. She was admitted to a medical facility."",
      ""after"": ""We agreed to optimize the dose to 30 mg daily."",
      ""description"": ""Corrected the dosage and removed unrelated content.""
    }}
  ]
}}";
        const string paulIn = $@"
Whisper Transcript:

I got a consultation on Paul Issa Dumais. Paul is P-A-U-L, Issa, I-S-S-A, Dumais, D-U-M-A-I-S. Consultation is 30 September 2024. Paul Issa Dumais is a 22-year-old gentleman who was assessed at Well Medica Clinic on 30 September 2024. He presented with a longstanding history of persistent sadness, lowering of mood, anhedonia, reduced energy, and poor motivation. He informed me he also has attention-focused difficulties starting from early childhood. He starts projects but is unable to finish them. He gets sidetracked and distracted quite easily. He is prone to procrastination, scattered and disorganized behavior, and can be late for appointments or assignments. He is forgetful, fidgety, and restless. He also has time management issues as well as executive function deficits. He experiences hyperactivity symptoms such as increased energy, overactivity, interrupting others, and being disruptive in class. He was concerned about potential hypo-mania or mania-like symptoms, but there was no evidence of pressured speech, flight of ideas, grandiosity, delusional thinking, or significant overactivity. He likely has hyperactivity symptoms as part of ADHD rather than actual bipolar disorder.

Past medical history: Nil of note.

Past psychiatric history: He has been diagnosed with major depressive disorder, anxiety disorder, and ADHD. He is currently maintained on Trintellix 15 mg daily along with Vyvanse 30 mg daily with some limited benefit.

Family history: Both parents are alive. He believes his father may have neurodivergence, and his mother has a history of anxiety. An uncle has schizophrenia. He is the only child.

Personal history: He was born and grew up in Toronto. Birth and early development were uneventful. He denied any birth-related injuries or early traumatic life experiences. He went to school and finished grade 12. He has been to university and completed a bachelor's degree. He is currently working in a kitchen as a chef. He is single and was in a relationship that ended recently.

Substance misuse history: He uses cannabis occasionally to help with anxiety symptoms.

Forensic history: Nil of note.

Mental status examination: Paul presents as a young gentleman who was pleasant and cooperative during the assessment. Speech was spontaneous and coherent. Mood was subjectively good. Objectively, he appeared mildly anxious. Thought form was normal. Thought content showed symptoms consistent with mixed anxiety, depression, and ADHD. He denied any paranoia, persecution, or auditory or visual hallucinations.

Diagnosis:
1. Major depression with anxiety.
2. ADHD, combined type.

Management Plan:
1. I had a discussion with Paul about his current medication regimen. I recommended optimizing the dose of Trintellix to 20 mg daily. If he continues to have problems, switching to Wellbutrin is an option.
2. I suggested optimizing the dose of Vyvanse, but he reported that he does not like its effects and finds it ineffective. Therefore, we agreed to discontinue Vyvanse and replace it with Concerta 18 mg daily for a week, followed by an increase to 36 mg daily.
3. I provided cognitive behavioural therapy along with psychoeducation.
4. I will review him again in six weeks' time.

Final Note:

Consult

Date: September 30, 2024

RE: Paul Issa-Dumais

HISTORY OF PRESENT ILLNESS

Paul Issa-Dumais is a 22-year-old gentleman who was assessed through Well Medica Clinic on September 30, 2024. He presented with a longstanding history of persistent sadness, lowering of mood, anhedonia, reduced energy, and poor motivation. He informed me that he also has attention and focus difficulties starting from earlier childhood. He starts a project and is not able to finish it. He gets sidetracked and distracted quite easily. He is prone to procrastination, scattered and disorganized behaviour, and can be late for appointments, assignments, forgetful, fidgety, and restless. He also has time management issues as well as executive function deficits. He also has hyperactivity symptoms with increased energy, overactivity, interrupting others, and being disruptive in class. He was concerned about hypo-mania or mania-like symptoms, but there was no evidence of pressured speech, flight of ideation, grandiosity, or delusional thinking. He likely has hyperactivity symptoms as part of ADHD rather than actual bipolar disorder.

PAST MEDICAL HISTORY

Nil of note.

PAST PSYCHIATRIC HISTORY

He has been diagnosed with major depressive disorder, anxiety disorder, and ADHD. He is currently maintained on Trintellix 15 mg daily along with Vyvanse 30 mg daily with some limited benefit.

FAMILY HISTORY

Both parents are alive. He believes his father may have neurodivergence and his mother has a history of anxiety. An uncle has schizophrenia. He is the only child.

PERSONAL HISTORY

He was born and grew up in Toronto. Birth and early development were uneventful. He denied any birth-related injuries or early traumatic life experiences. He went to school and finished grade 12. He has been to university and completed a bachelor's degree. He is also working in a kitchen as a chef. He is single and was in a relationship that ended recently.

SUBSTANCE MISUSE HISTORY

He uses cannabis occasionally to help with anxiety symptoms.

FORENSIC HISTORY

Nil of note.

MENTAL STATUS EXAMINATION

Paul presents as a young gentleman who was pleasant and cooperative during the assessment. Speech was spontaneous and coherent. Mood was subjectively good. Objectively, he appeared mildly anxious. Thought form was normal. Thought content showed symptoms consistent with mixed anxiety, depression, and ADHD. He denied any paranoia, persecution, or auditory or visual hallucinations.

DIAGNOSES

1. Major depression with anxiety.
2. ADHD, combined type.

MANAGEMENT PLAN

1. I had a discussion with Paul about his current medication regimen. I recommended optimizing the dose of Trintellix to 20 mg daily. If he continues to have problems, switching to Wellbutrin is an option.
2. I suggested optimizing the dose of Vyvanse, but he reported that he does not like its effects and finds it ineffective. Therefore, we agreed to discontinue Vyvanse and replace it with Concerta 18 mg daily for a week, followed by an increase to 36 mg daily.
3. I provided cognitive behavioural therapy along with psychoeducation.
4. I will review him again in six weeks' time.

**DICTATED BUT NOT READ**

kb";
        const string paulOut = $@"
{{
  ""updated_transcript"": ""Psychiatric consultation on Paul Issa-Dumais. Paul is P-A-U-L, Issa, I-S-S-A, Dumais, D-U-M-A-I-S. Consultation date is the 30th of September, 2024. Paul Issa-Dumais is a 22-year-old gentleman who was assessed at Well Medica Clinic on the 30th of September, 2024. He presented with a longstanding history of persistent sadness, lowering of mood, anhedonia, reduced energy, and poor motivation. He informed me that he also has attention and focus difficulties starting from earlier childhood. He starts projects but is not able to finish them, gets sidetracked and distracted easily, and is prone to procrastination, scattered and disorganized behavior. He can be late for appointments, forgetful, fidgety, and restless. He also has time management issues and executive function deficits. He experiences hyperactivity symptoms, such as increased energy, overactivity, interrupting others, and being disruptive in class. He was concerned about possible hypomania or mania-like symptoms, but there was no clear evidence of pressured speech, flight of ideas, grandiosity, or delusional thinking. These hyperactivity symptoms are likely part of ADHD rather than bipolar disorder.\n\nNext, past medical history. Nil of note.\n\nNext, past psychiatric history. He has been diagnosed with major depressive disorder, anxiety disorder, and ADHD. He is currently maintained on Trintellix 15 mg daily along with Vyvanse 30 mg daily, with some limited benefit.\n\nNext, family history. Both parents are alive. He believes his father may have neurodivergence, and his mother has a history of anxiety. An uncle has schizophrenia. He is the only child in the family.\n\nNext, personal history. He was born and grew up in Toronto. Birth and early development were uneventful. He denied any birth-related injuries or early traumatic life experiences. He went to school and finished grade 12. He has been to university and completed a bachelor's degree. He is currently working as a chef in a kitchen. He is single and was in a relationship that recently ended.\n\nNext, substance misuse history. He occasionally uses cannabis to help with anxiety symptoms.\n\nNext, forensic history. Nil of note.\n\nNext, mental status examination. Paul presents as a young gentleman who was pleasant and cooperative during the assessment. Speech was spontaneous and coherent. Mood was subjectively good. Objectively, he appeared mildly anxious. Thought form was normal. Thought content showed symptoms consistent with mixed anxiety and depression and ADHD. He denied any paranoia, persecution, or auditory or visual hallucinations.\n\nNext, diagnoses. 1. Major depression with anxiety. 2. ADHD, combined type.\n\nNext, management plan. 1. I had a discussion with Paul about his current medication regimen, and I recommended optimizing the dose of Trintellix to 20 mg daily. If he continues to have problems, switching to Wellbutrin would be an option. 2. We discussed discontinuing Vyvanse as he does not like its effects and replacing it with Concerta, starting at 18 mg daily for a week and increasing to 36 mg daily. 3. I provided cognitive behavioural therapy along with psychoeducation. 4. I will review him again in six weeks' time."",
  ""final_note"": ""Consult\n\nDate: September 30, 2024\n\nRE: Paul Issa-Dumais\n\nHISTORY OF PRESENT ILLNESS\n\nPaul Issa-Dumais is a 22-year-old gentleman who was assessed through Well Medica Clinic on September 30, 2024.  He presented with a longstanding history of persistent sadness, lowering of mood, anhedonia, reduced energy, and poor motivation.  He informed me that he also has attention and focus difficulties starting from earlier childhood.  He starts a project and is not able to finish it.  He gets sidetracked and distracted quite easily.  He is prone to procrastination, scattered and disorganized behaviour and can be late for appointments, assignments, forgetful, fidgety, and restless.  He also has time management issues as well as executive function deficits.  He also has hyperactivity symptoms with increased energy, overactivity, interrupting others, and disruptive in the class.  He was concerned whether he has any hypo-mania or mania-like symptoms, but it was not clear whether there were any mania-like symptoms with any pressured Speech was spontaneous and coherent or flight of ideation or overactivity or grandiosity or any delusional thinking.  He probably has a bit of hyperactivity symptoms as part of the ADHD rather than an actual bipolar disorder.\n\nPAST MEDICAL HISTORY\n\nNil of note.\n\nPAST PSYCHIATRIC HISTORY\n\nHe has been diagnosed with major depressive disorder, anxiety disorder, and ADHD.  He is currently maintained on Trintellix 15 mg daily along with Vyvanse 30 mg daily with some limited benefit.\n\nFAMILY HISTORY\n\nBoth parents are alive.  He believes father maybe has neurodivergence and mother has a history of anxiety.  An uncle has schizophrenia.  He is the only child.\n\nPERSONAL HISTORY\n\nHe was born and grew up in Toronto.  Birth and early development were uneventful.  He denied any birth-related injuries or early traumatic life experiences.  He went to school and finished grade 12.  He has been to university and has completed a bachelor's degree.  He is also working in a kitchen as a chef.  He is single.  He was in a relationship before which ended recently.\n\nSUBSTANCE MISUSE HISTORY\n\nHe does use cannabis from time to time to help with anxiety symptoms.\n\nFORENSIC HISTORY\n\nNil of note.\n\nMENTAL STATUS EXAMINATION\n\nPaul presents as a young gentleman who was pleasant and cooperative during the assessment.  Speech was spontaneous and coherent.  Mood was subjectively good.  Objectively, he appeared mildly anxious.  Thought form was normal.  Thought content showed symptoms consistent with mixed anxiety and depression and ADHD.  He denied any paranoia, persecution, or auditory or visual hallucinations.\n\nDIAGNOSES\n\n1. Major depression with anxiety.\n2. ADHD, combined type.\n\nMANAGEMENT PLAN\n\n1. I had a discussion with Paul about his current regimen of medication and I would recommend optimizing the dose of Trintellix to 20 mg daily and if he is still having some problems, then switching it to Wellbutrin will be an option.\n\n2. I will suggest optimizing the dose of Vyvanse but he informed me that he does not like the effects of it.  It has not made any difference.  Therefore, we agreed to discontinue the Vyvanse and replace it with Concerta 18 mg daily for a week followed by increase to 36 mg daily.\n\n3. I provided some cognitive behavioural therapy along with psychoeducation.\n\n4. I will review him again in six weeks' time.\n\n**DICTATED BUT NOT READ**\n\nkb"",
  ""change_summary"": [
    {{
      ""before"": ""assessed to World Medical Clinic"",
      ""after"": ""assessed at Well Medica Clinic"",
      ""description"": ""Corrected the clinic name and phrasing to match the final note.""
    }},
    {{
      ""before"": ""coma, and hedonia"",
      ""after"": ""anhedonia"",
      ""description"": ""Corrected transcription error to reflect proper medical terminology.""
    }},
    {{
      ""before"": ""15 mg daily along with the Y-vans, 30 mg, 30 mg daily"",
      ""after"": ""Trintellix 15 mg daily along with Vyvanse 30 mg daily"",
      ""description"": ""Corrected medication names and dosage formatting.""
    }},
    {{
      ""before"": ""he informed me that it is not liking the effects of it"",
      ""after"": ""he informed me that he does not like the effects of it"",
      ""description"": ""Adjusted phrasing for clarity and grammatical correctness.""
    }},
    {{
      ""before"": ""replace it with a conservative 1.8 mg daily for a week for a increase to 36 mg daily."",
      ""after"": ""replace it with Concerta 18 mg daily for a week followed by an increase to 36 mg daily."",
      ""description"": ""Corrected medication name and dosage formatting.""
    }}
  ]
}}";
        const string sasikalaIn = $@"
Whisper Transcript:

Psychiatric consultation with Sasikala Sritharan. Sasikala is S-A-S-I-K-A-L-A, Sritharan, S-R-I-T-H-A-R-A-N. Consultation date: 30th September 2024. Sasikala Sritharan is a 48-year-old Sri Lankan-Canadian lady who was assessed at Well Medica Clinic on the 30th of September, 2024. She has been known to me with a history of major depression, anxiety, PTSD, and chronic pain. She was maintained on a complicated regimen of medication which included Cymbalta 120 mg, Seroquel 250 mg, and Trazodone. Her liver function tests showed fatty liver and deranged liver functions. Therefore, she was taken off all her medications. 

Now, she is struggling as she is not able to sleep well. She was provided with psychoeducation regarding the risks and benefits of medication. Most medications are metabolized through the liver, potentially affecting liver function. It is in her interest to avoid medication at this stage and rely on psychosocial interventions, including physical exercise, structured activities, and social stimulation, so that she relies on endorphins rather than stress hormones.

Mental status examination: Sasikala presents as a lady who was anxious throughout the assessment. Mood was subjectively low. Objectively, she appeared anxious and depressed. Thought form was normal. Thought content showed symptoms consistent with major depressive disorder and PTSD. She denied any paranoia, persecution, or auditory or visual hallucinations.

Diagnosis:
1. Major depressive disorder.
2. Generalized anxiety disorder.
3. PTSD.

Management Plan:
1. Sasikala is currently off all medications, including Cymbalta, Seroquel, Trazodone, and terazosin. If she continues to have anxiety, she has the option of using Ativan on an emergency basis. If she decides to go back on antidepressants, sertraline 25 mg daily can be started, increasing gradually. However, a reprieve from medication is recommended to allow her liver function to improve.
2. I provided her with cognitive behavioural therapy, psychoeducation, and interpersonal therapy to support her through this life transition.
3. I will review her again in two to three months' time.

Final Note:

Consult

Date: September 30, 2024

RE: Sasikala Sritharan

HISTORY OF PRESENT ILLNESS

Sasikala Sritharan is a 48-year-old, Sri Lankan-Canadian lady who was assessed through Well Medica Clinic on September 30, 2024. She has been known to me with a history of major depression with anxiety, PTSD, and chronic pain. She was maintained on quite a complicated regimen of medication which included Cymbalta 120 mg, Seroquel 250 mg, and Trazodone. Her liver function tests showed fatty liver and deranged liver functions. Therefore, she was taken off all her medications. Now, she is struggling as she is not able to sleep well. She was given psychoeducation regarding the risks and benefits of using medications. Most medications are metabolized and can affect liver function, so it is in her interest to avoid medication at this stage and rely on psychosocial interventions such as physical exercise, structured activities, and social stimulation, enabling her to utilize endorphins rather than stress hormones.

MENTAL STATUS EXAMINATION

Sasikala presents as a lady who was anxious throughout the assessment. Mood was subjectively low. Objectively, she appeared anxious and depressed. Thought form was normal. Thought content showed symptoms consistent with major depressive disorder and PTSD. She denied any paranoia, persecution, or auditory or visual hallucinations.

DIAGNOSES

1. Major depressive disorder.
2. Generalized anxiety disorder.
3. PTSD.

MANAGEMENT PLAN

1. Sasikala is currently off all medications, including Cymbalta, Seroquel, Trazodone, and terazosin. If she continues to have anxiety, she has the option of using Ativan on an emergency basis. If she decides to go back on an antidepressant, then sertraline 25 mg daily can be initiated, increasing gradually. However, a reprieve from medication is recommended to allow her liver function to improve.
2. I provided her with cognitive behavioural therapy along with psychoeducation and interpersonal therapy to help with her life transition.
3. I will review her again in two to three months' time.

**DICTATED BUT NOT READ**

kb";
        const string sasikalaOut = $@"
{{
  ""updated_transcript"": ""Psychiatric consultation with Sasikala Sritharan. Sasikala is S-A-S-I-K-A-L-A, Sritharan, S-R-I-T-H-A-R-A-N. Consultation date is the 30th of September, 2024. Sasikala Sritharan is a 48-year-old Sri Lankan-Canadian lady who was assessed at Well Medica Clinic on the 30th of September, 2024. She has a history of major depression with anxiety, PTSD, and chronic pain. She was maintained on a complicated regimen of medication, including Cymbalta 120 mg, Seroquel 250 mg, and Trazodone. Her liver function test revealed a fatty liver and deranged liver functions. As a result, she was taken off all medications. Currently, she is struggling with sleep issues. She was provided with psychoeducation about the risks and benefits of medication use. Most medications are metabolized in the liver and can affect its function. It would be in her best interest to avoid medications at this stage and focus on psychosocial interventions, including physical exercise, structured activities, and social stimulation, to rely on endorphins rather than stress hormones.\n\nNext, mental status examination. Sasikala presents as a middle-aged lady who appeared anxious throughout the assessment. Mood was subjectively low, and objectively, she appeared anxious and depressed. Thought form was normal. Thought content showed symptoms consistent with major depressive disorder and PTSD. She denied any paranoia, persecution, or auditory or visual hallucinations.\n\nNext, diagnoses. 1. Major depressive disorder. 2. Generalized anxiety disorder. 3. PTSD.\n\nNext, management plan. 1. Sasikala is currently off all medications, including Cymbalta, Seroquel, Trazodone, and terazosin. If her anxiety persists, she has the option of using Ativan on an emergency basis. Alternatively, if she wants to restart an antidepressant, sertraline 25 mg daily can be initiated with a gradual dose increase. However, it is preferable to allow a medication reprieve to support liver function recovery. 2. I provided her with cognitive behavioural therapy along with psychoeducation and interpersonal therapy to assist with life transitions. 3. I will review her again in two to three months' time."",
  ""final_note"": ""Consult\n\nDate: September 30, 2024\n\nRE: Sasikala Sritharan\n\nHISTORY OF PRESENT ILLNESS\n\nSasikala Sritharan is a 48-year-old, Sri Lankan-Canadian lady who was assessed through Well Medica Clinic on September 30, 2024.  She has been known to me with a history of major depression with anxiety, PTSD, and chronic pain.  She was maintained on quite a complicated regimen of medication which included Cymbalta 120, Seroquel 250, and Trazodone, and she had her liver function test done and it showed that she had a fatty liver and deranged live functions.  Therefore, she was taken off all the medication.  Now, she is struggling as she is not able to sleep well.  She was given some psychoeducation regarding risks and benefits of using the medication.  Most medications do get metabolized and affect liver function, so I think it will be in her interest to do without medication at this stage and rely on psychosocial intervention including physical exercise and structured activities and social stimulation so that she is using endorphins rather than a stress hormone.\n\nMENTAL STATUS EXAMINATION\n\nSasikala presents as a young lady who was anxious throughout the assessment.  Mood was subjectively low.  Objectively, she appeared anxious and depressed.  Thought form was normal.  Thought content showed symptoms consistent with major depressive disorder and PTSD.  She denied any paranoia, persecution, or auditory or visual hallucinations.\n\nDIAGNOSES\n\n1. Major depressive disorder.\n2. Generalized anxiety disorder.\n3. PTSD.\n\nMANAGEMENT PLAN\n\n1. Sasikala is currently off the medication including Cymbalta, Seroquel, Trazodone, and terazosin.  If she continues to have anxiety, either she has the option of using some Ativan only on an emergency basis or if she wants to go back on an antidepressant, then SSRI, either the sertraline 25 mg daily starting with a small dose then building it up, but ideally she should have a reprieve from the medication so that if the liver functions have been impaired because of the medication, it will help her to improve her liver function.\n\n2. I provided her with cognitive behavioural therapy along with psychoeducation as well as interpersonal therapy to help with the life transition.\n\n3. I will review her again in two to three months' time.\n\n**DICTATED BUT NOT READ**\n\nkb"",
  ""change_summary"": [
    {{
      ""before"": ""assessed to World Medical Clinic"",
      ""after"": ""assessed at Well Medica Clinic"",
      ""description"": ""Corrected the clinic name and phrasing to match the final note.""
    }},
    {{
      ""before"": ""Cibolta 120, Seroquel 250 and Triazidone"",
      ""after"": ""Cymbalta 120 mg, Seroquel 250 mg, and Trazodone"",
      ""description"": ""Corrected spelling and formatted medication names and dosages accurately.""
    }},
    {{
      ""before"": ""Fulstra"",
      ""after"": ""stress hormones"",
      ""description"": ""Rephrased unclear or incorrect transcription for clarity.""
    }},
    {{
      ""before"": ""Mood was subject to low blood pressure but anxious and passed out form"",
      ""after"": ""Mood was subjectively low, and objectively, she appeared anxious and depressed."",
      ""description"": ""Corrected nonsensical phrasing to align with clinical terminology.""
    }},
    {{
      ""before"": ""schizophrenia, paranoia, persecution, auditory visual hallucinations"",
      ""after"": ""major depressive disorder and PTSD. She denied any paranoia, persecution, or auditory or visual hallucinations."",
      ""description"": ""Clarified thought content and excluded unrelated or erroneous terms.""
    }},
    {{
      ""before"": ""Triazidone and Triazosin Fulstra"",
      ""after"": ""Trazodone and terazosin"",
      ""description"": ""Corrected transcription errors in medication names.""
    }}
  ]
}}";
        const string noteRequest = $@"Fantastic work comparing the transcripts with the final notes and identifying consistencies and differences! Now that you have a strong understanding of both the erroneous transcript and the finalized consult notes, I’d like to assign your next task. I will provide you with transcripts containing similar errors due to pronunciation and transcription challenges. Your job will be to create notes that mirror the exact form and function of the previous ones. Please provide the note in plain-text format.

In the next prompt, I will supply a transcript, and I expect you to output the corresponding note—nothing extra, so I can directly copy and paste it into my EMR. Ensure that no details are missed. Before we begin, I’m sharing an analysis based on our prior work to guide you in generating accurate notes. Good luck, and let’s get started!

**Detailed Analysis of Transcripts and Notes**

### **Common and Consistent Errors**

1. **Medication Names and Dosages:**

   - Medications were frequently misinterpreted (e.g., ""Y1's"" instead of ""Vyvanse,"" ""Triazidone"" instead of ""Trazodone"").
   - Dosage formatting was inconsistent, such as ""20-20mg daily"" instead of ""20 mg daily.""

2. **Medical Terminology:**

   - Misused terms (e.g., ""coma and hedonia"" instead of ""anhedonia"").
   - Confusion between similar terms (e.g., ""paranoia, persecution"" misaligned with ""denied paranoia or persecution"").

3. **Grammar and Sentence Structure:**

   - Frequent run-on sentences and grammatical errors (e.g., ""Mood was subject to record. Objective she was mildly anxious."").
   - Sentence fragments or nonsensical phrases (e.g., ""Mood was subject to low blood pressure but anxious and passed out form"").

4. **Inconsistent Formatting:**

   - Patient identifiers were formatted inconsistently.
   - Transition phrases and section headings (e.g., ""Next in the medical examination"" versus ""Next, mental status examination"") lacked uniformity.

5. **Spelling Errors:**

   - Repeated misspellings (e.g., ""Cibolta"" for ""Cymbalta,"" ""tuberular adenoma"" for ""tubular adenoma"").


6. **Spelling Errors:**

   - The spelling of the clinic referenced is at times misinterpreted. For example it can sometimes appear as 'World Medical Clinic' instead of 'Well Medica Clinic'. Please note the clinic referenced is always 'Well Medica Clinic'.

7. **Duplication:**

   - Redundant phrases or repeated information (e.g., ""She was born and grew up in Iran. Birth and early development were uneventful."" repeated unnecessarily).

---

### **Function of the Notes/Dictations**

1. **Clinical Documentation:**

   - The primary function is to capture a comprehensive account of the patient’s psychiatric consultation, including history, examination findings, diagnoses, and management plans.

2. **Patient Care:**

   - Provides an accurate and detailed record to guide ongoing treatment, ensuring continuity of care.
   - Offers clear documentation for interdisciplinary teams, aiding in collaborative care.

3. **Record-Keeping:**

   - Serves as a legal and professional record of the consultation.
   - Ensures compliance with documentation standards in clinical practice.

---

### **General Flow of the Transcripts**

1. **Patient Identification:**

   - Begins with the patient’s name, spelling, and consultation date.

2. **History of Present Illness:**

   - Describes presenting complaints, symptom chronology, and relevant medical/psychiatric history.

3. **Past Medical and Psychiatric History:**

   - Details medical conditions, psychiatric diagnoses, and current medications.

4. **Family and Personal History:**

   - Covers familial conditions, patient’s upbringing, education, relationships, and substance use.

5. **Mental Status Examination:**

   - Evaluates mood, affect, thought form, and content, with consistent mention of symptoms like anxiety and depression.

6. **Diagnoses:**

   - Summarizes the conditions identified during the assessment.

7. **Management Plan:**

   - Provides recommendations for medication adjustments, therapy, and follow-up plans.

---

### **Physician's Style and Consistencies**

1. **Structured Sections:**

   - Dictation follows a consistent sequence: identification, history, examination, diagnosis, and management.

2. **Preferred Terms:**

   - Regular use of phrases like ""Mood was subjectively low,"" ""Thought form was normal,"" and ""Symptoms consistent with...""

3. **Transition Phrases:**

   - Sections are introduced with predictable transitions, such as ""Next, personal history,"" or ""Next, management plan.""

4. **Emphasis on Psychoeducation:**

   - Frequent discussions on risks and benefits of medication and reliance on psychosocial interventions.

5. **Detailed Medication Plans:**

   - Thorough explanation of medication adjustments, dosages, and alternatives.

---

### **Notes Structure**

1. **History of Present Illness:**

   - Comprehensive overview of the patient’s symptoms, treatment history, and current challenges.

2. **Mental Status Examination:**

   - Detailed observations of mood, thought process, and behavior.

3. **Diagnoses:**

   - Clear enumeration of the patient’s psychiatric conditions.

4. **Management Plan:**

   - Well-structured treatment recommendations, including medication adjustments, therapy, and follow-up.

5. **Miscellaneous Sections:**

   - Personal and family history, substance misuse history, and forensic history where applicable.

6. **Signature:**

   - The physician consistently concludes each note with DICTATED BUT NOT READ and signs off with ""kb"" as a standard closing.

---

### **Guidelines and Observations for Future Workflow**

1. **Note Content:**

   - The consult note content comprehensively captures all the details included in the transcript.

2. **Patient Name spelling Error:**

   - Transcriptions often capture the spelling of the patient’s name incorrectly. However, the physician consistently spells out the name letter by letter at the start of the consultation (e.g., ""Arian is A-R-I-E-N Bryson B-R-Y-S-E-N Russell R-U-S-S-E-L-L""). Use the physician’s spelled-out version (e.g., ""Arien Brysen Russell"") in the final note to ensure accuracy, but ensure the spelled-out letters are not included in the note. 

3. **Standardized Formatting:**

   - Consistent section headings and transitions.
   - Standardize medication and dosage formatting (e.g., ""25 mg daily"" rather than ""25mg daily"").

4. **Terminology Accuracy:**

   - Verify medication names and medical terms against a reference list of commonly referred to drug names: 

     - Sertraline, Seroquel, Trintellix, Trazodone, Prazosin, Paroxetine, Fluoxetine, Clomipramine, Vyvanse, Cymbalta, Adderall, Strattera, Wellbutrin, Sublinox, Clonazepam, Ativan, Mirtazapine, Effexor, Cipralex, Prozac, Abilify, Concerta. 
     - Note: There may be other drugs but these are the most common ones referred to.

   - &#x20;

     Avoid vague or nonsensical phrases.

5. **Grammar and Sentence Structure:**

   - Use complete, grammatically correct sentences.
   - Avoid redundancy and ensure logical flow.

6. **Patient Identification:**

   - Always spell out names and confirm proper formatting at the start.

7. **Medication Adjustments:**

   - Provide clear reasoning for changes and specify incremental dosage adjustments.

8. **Consistency in Thought Form and Content:**

   - Clearly state findings (e.g., ""Thought form was normal. Thought content showed symptoms consistent with..."").

9. **Emphasis on Psychoeducation:**

   - Highlight the importance of patient education and psychosocial strategies.

10. **Review and Follow-Up:**

    - Specify exact timelines for follow-up appointments.

---
";
        const string noteRequestResponse = "Understood! Please provide the Whisper Transcript and the Final Scribe Note, and I will proceed with identifying and correcting transcription errors, following the structure you've outlined for the updated transcript, the unchanged final note, and a detailed change summary.";


        public MainWindow()
        {
            InitializeComponent();

            // Initialize timer
            timer = new System.Timers.Timer(1000); // 1-second intervals
            timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Update the elapsed time
            elapsedTime = elapsedTime.Add(TimeSpan.FromSeconds(1));

            // Update the timer display on the UI thread
            Dispatcher.Invoke(() =>
            {
                TimerDisplay.Text = elapsedTime.ToString(@"hh\:mm\:ss");
            }); 

        }

        private void ListBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            // Allow dragging only files
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Any(file => _supportedExtensions.Contains(Path.GetExtension(file).ToLower())))
                {
                    e.Effects = DragDropEffects.Copy;
                    e.Handled = true;
                    return;
                }
            }

            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Filter files for supported extensions and add to ListBox
                foreach (var file in files)
                {
                    if (_supportedExtensions.Contains(Path.GetExtension(file).ToLower()))
                    {
                        if (sender is ListBox listBox)
                        {
                            listBox.Items.Add(file);
                        }
                    }
                }
            }
        }

        private string GetStorePathFromIni(string iniFilePath)
        {
            // Read all lines from the ini file
            var lines = File.ReadAllLines(iniFilePath);

            foreach (var line in lines)
            {
                if (line.StartsWith("Store"))
                {
                    // Parse the Store path from the ini file
                    var keyValue = line.Split('=');
                    if (keyValue.Length == 2)
                    {
                        return keyValue[1].Trim(); // Return the path after '='
                    }
                }
            }

            // Default fallback if Store key is not found
            throw new Exception("Store path not found in ini file.");
        }

        private async Task<string> TranscribeAudio(string audioFilePath)
        {
            string transcriptFile = "";
            string wavFilePath = "";
            try
            {
                // Path to your ini file
                if (Path.GetExtension(audioFilePath).ToLower() == ".aac") {
                    wavFilePath = audioFilePath.Replace(".aac", ".wav");
                    AudioConverter.ConvertAACToWAV(audioFilePath, wavFilePath);
                    audioFilePath = wavFilePath;
                }

                var client = new RestClient("https://api.openai.com/v1/audio");
                var request = new RestRequest("transcriptions", Method.Post);
                request.AddHeader("Authorization", "Bearer sk-proj-5jUdp2BeY1l5xsT86t43T3BlbkFJXteaBCbpUsQjEZM0XQk8");
                request.AddFile("file", audioFilePath);
                request.AddParameter("model", "whisper-1");

                // Escape special characters for SendKeys
                RestResponse response = await client.ExecuteAsync(request);
                
                if (response.IsSuccessful) {
                    var transcription = response.Content; // Adjust according to the API response structure
                    var jsonObject = JObject.Parse(transcription);
                    string extractedText = jsonObject["text"]?.ToString();
                    // Write the transcribed text to a file
                    // Create a "generated" folder within the same directory as audioFilePath
                    string directoryPath = Path.GetDirectoryName(audioFilePath);
                    string generatedFolder = Path.Combine(directoryPath, "generated");

                    if (!Directory.Exists(generatedFolder))
                    {
                        Directory.CreateDirectory(generatedFolder);
                    }

                    // Define the transcript file path in the "generated" folder
                    transcriptFile = Path.Combine(generatedFolder, Path.GetFileNameWithoutExtension(audioFilePath) + ".transcript.txt");
                    
                    // Write the transcribed text to the file
                    System.IO.File.WriteAllText(transcriptFile, extractedText);

                    Console.WriteLine($"Transcription saved to: {transcriptFile}");
                    if (File.Exists(wavFilePath))
                    {
                        try
                        {
                            File.Delete(wavFilePath);
                            Console.WriteLine($"Deleted: {wavFilePath}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"An error occurred: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("The specified file does not exist.");
                    }


                }
                else {
                    throw new Exception(response.Content);
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }

            return transcriptFile;
        }

        private async Task<String> GenerateNoteFromAudio(string AudioFile)
        {
            string TranscriptionFile = await TranscribeAudio(AudioFile);
            string NoteFile = await CreateNote(TranscriptionFile);
            return NoteFile;
        }

        /// <summary>
        /// Slows down a WAV file by the specified speed factor.
        /// </summary>
        /// <param name="inputPath">Path to the input WAV file.</param>
        /// <param name="outputPath">Path to save the slowed-down WAV file.</param>
        /// <param name="speedFactor">The factor by which to slow down the audio (e.g., 0.85).</param>
        static void SlowDownAudio(string inputPath, string outputPath, double speedFactor)
        {
            if (speedFactor <= 0)
                throw new ArgumentException("Speed factor must be greater than 0.");

            using (var reader = new AudioFileReader(inputPath))
            {
                int originalSampleRate = reader.WaveFormat.SampleRate;
                int slowedSampleRate = (int)(originalSampleRate * speedFactor);

                var resampler = new MediaFoundationResampler(reader, new WaveFormat(slowedSampleRate, reader.WaveFormat.Channels));
                resampler.ResamplerQuality = 60; // High quality

                WaveFileWriter.CreateWaveFile(outputPath, resampler);
            }
        }

        private async Task<string> CreateNote(string transcriptFile)
        {
            string outputNotePath = "";
            if (string.IsNullOrWhiteSpace(transcriptFile))
            {
                throw new ArgumentException("Transcript file path cannot be null or empty.", nameof(transcriptFile));
            }
            if (!File.Exists(transcriptFile))
            {
                throw new FileNotFoundException("The specified transcript file does not exist.", transcriptFile);
            }
            try
            {
                // Read the transcript content asynchronously
                string transcriptContent = await File.ReadAllTextAsync(transcriptFile);

                if (string.IsNullOrWhiteSpace(transcriptContent))
                {
                    Console.WriteLine("The transcript file is empty. No note will be created.");
                    return outputNotePath;
                }

                // GPT-4 API endpoint
                var client = new RestClient("https://api.openai.com/v1/chat/completions");
                var request = new RestRequest();
                request.Method = Method.Post;
                request.AddHeader("Authorization", "Bearer sk-proj-5jUdp2BeY1l5xsT86t43T3BlbkFJXteaBCbpUsQjEZM0XQk8"); // Replace with your GPT-4 API key
                // Prepare the input for GPT-4
                var body = new
                {
                    model = "gpt-4o",
                    messages = new[]
                    {
                        new
                        {
                            role = "system",
                            content = "You are a helpful assistant specializing in clinical documentation analysis and refinement, designed to identify patterns, ensure consistency, correct errors, and provide detailed, structured insights for improving the accuracy and clarity of medical consult notes."
                        },
                        new
                        {
                            role = "user",
                            content = "You are tasked with creating structured and accurate updates to medical dictation transcripts for Dr. Haque. Below are key details and instructions:\r\n\r\nContext:\r\n\r\nThe provided transcripts originate from audio recordings of Dr. Haque's dictations, processed using a speech recognition API (Whisper).\r\nDr. Haque speaks with a thick Indian accent and at a rapid pace, which causes frequent transcription errors.\r\nPreviously, a human scribe familiar with Dr. Haque's speech patterns and consistent documentation structure corrected these transcripts and produced final consult notes for records. However, the scribe is currently unavailable.\r\nProvided Data:\r\n\r\nWhisper Transcript: The raw, machine-generated text, prone to errors.\r\nFinal Scribe Note: The accurate and polished note created by the human scribe.\r\nYour Task:\r\n\r\nUsing the Whisper Transcript and the Final Scribe Note, reverse-engineer an updated and corrected transcript that resolves all transcription errors.\r\nLeverage your knowledge of clinical documentation, medical terminology, and Dr. Haque’s consistent note structure to make accurate corrections.\r\nOutput Format:\r\nProduce a JSON object containing the following:\r\n\r\nupdated_transcript: The corrected version of the Whisper transcript.\r\nfinal_note: The original final note for reference (unchanged).\r\nchange_summary: A list of transcription changes with:\r\nbefore: The erroneous text.\r\nafter: The corrected text.\r\ndescription: A brief explanation of the correction (e.g., \"Corrected medical term\", \"Fixed grammar\", \"Clarified ambiguous phrasing\").\r\nNext Steps:\r\n\r\nOnce this task is complete, your findings will be compiled to create accurate consult notes directly from Whisper transcriptions in future tasks.\r\n\r\nThank you for your assistance! Your precision and attention to detail are greatly appreciated."
                        },
                        new
                        {
                            role = "assistant",
                            content = "Understood! Please provide the Whisper Transcript and the Final Scribe Note, and I will proceed with identifying and correcting transcription errors, following the structure you've outlined for the updated transcript, the unchanged final note, and a detailed change summary."
                        },
                        new
                        {
                            role = "user",
                            content = faribaIn
                        },
                        new
                        {
                            role = "assistant",
                            content = faribaOut
                        },
                        /*new
                        {
                            role = "user",
                            content = meganIn
                        },
                        new
                        {
                            role = "assistant",
                            content = meganOut
                        },
                        new
                        {
                            role = "user",
                            content = arienIn
                        },
                        new
                        {
                            role = "assistant",
                            content = arienOut
                        },
                        new
                        {
                            role = "user",
                            content = emelinaIn
                        },
                        new
                        {
                            role = "assistant",
                            content = emelinaOut
                        },
                        new
                        {
                            role = "user",
                            content = ashleyIn
                        },
                        new
                        {
                            role = "assistant",
                            content = ashleyOut
                        }, */
                        new
                        {
                            role = "user",
                            content = paulIn
                        },
                        new
                        {
                            role = "assistant",
                            content = paulOut
                        },
                        new
                        {
                            role = "user",
                            content = sasikalaIn
                        },
                        new
                        {
                            role = "assistant",
                            content = sasikalaOut
                        },
                        new
                        {
                            role = "user",
                            content = noteRequest
                        },
                        new
                        {
                            role = "assistant",
                            content = noteRequestResponse
                        },
                        new
                        {
                            role = "user",
                            content = transcriptContent
                        }
                        },
                    temperature = 0.7 // Adjust temperature for creativity
                };




                // Add body to the request
                request.AddJsonBody(body);

                // Send the request and get the response
                RestResponse response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    // Parse the GPT-4 response
                    var gptResponse = JObject.Parse(response.Content);
                    string generatedNote = gptResponse["choices"]?[0]?["message"]?["content"]?.ToString();

                    if (!string.IsNullOrEmpty(generatedNote))
                    {
                        var titleRequest = new RestRequest();
                        titleRequest.Method = Method.Post;
                        titleRequest.AddHeader("Authorization", "Bearer sk-proj-5jUdp2BeY1l5xsT86t43T3BlbkFJXteaBCbpUsQjEZM0XQk8"); // Replace with your GPT-4 API key
                        // Save the medical note to a file
                        // Extract a formatted title
                        var titleBody = new
                        {
                            model = "gpt-4",
                            messages = new[]
                            {
                                new { role = "system", content = "You are an assistant specializing in structured data extraction." },
                                new { role = "user", content = $"Extract the patient's name and date from the following note into the format 'FirstName-LastName-MM-DD-YYYY'. If not present, return nothing. Note: {generatedNote}" }
                            },
                            temperature = 0.7
                        };
                        
                        titleRequest.AddJsonBody(titleBody);
                        RestResponse titleResponse = await client.ExecuteAsync(titleRequest);

                        var titleResponseContent = JObject.Parse(titleResponse.Content);
                        string generatedTitle = titleResponseContent["choices"]?[0]?["message"]?["content"]?.ToString();

                        // Generate output file path
                        outputNotePath = !string.IsNullOrWhiteSpace(generatedTitle)
                            ? Path.Combine(Path.GetDirectoryName(transcriptFile), $"{generatedTitle}.txt")
                            : transcriptFile.Replace("transcript.txt", "note.txt");

                        System.IO.File.WriteAllText(outputNotePath, generatedNote);

                        string renamedTranscriptFilePath = !string.IsNullOrWhiteSpace(generatedTitle) ? Path.Combine(Path.GetDirectoryName(transcriptFile), $"{generatedTitle}_transcript.txt") : transcriptFile; // Fallback to the original transcript file name if no title
                        if (File.Exists(transcriptFile))
                        {
                            File.Move(transcriptFile, renamedTranscriptFilePath);
                            Console.WriteLine($"Transcript file renamed to: {renamedTranscriptFilePath}");
                        }
                        Console.WriteLine($"Medical note created and saved to: {outputNotePath}");
                        return outputNotePath;

                    }
                    else
                    {
                        Console.WriteLine("Failed to generate a medical note. No content returned.");
                    }
                } else {
                    // Handle API errors
                    throw new Exception($"GPT-4.0 API error: {response.Content}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while creating the note: {ex.Message}");
            }
            return outputNotePath;

        }


        /// <summary>
        /// Reads the content of Prompt.txt from the Debug folder and returns it as a string.
        /// </summary>
        /// <returns>The content of the file, or null if an error occurs.</returns>
        static string ReadPromptFile()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Prompt.txt");

            try
            {
                // Read the file content
                return File.ReadAllText(filePath);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Error: File not found at {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            // Return null if there was an error
            return null;
        }
        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            // Write audio data to the file
            if (waveFile != null)
            {
                waveFile.Write(e.Buffer, 0, e.BytesRecorded);
                waveFile.Flush();
            }
        }

        private void WaveIn_RecordingStopped(object sender, StoppedEventArgs e)
        {
            // Dispose of the recorder and writer
            waveIn.Dispose();
            waveFile.Dispose();
            waveIn = null;
            waveFile = null;

            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Recording.wav");
            
            //Todo Eugene: Add in Transformer here.
        }

        private void StartRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                waveIn = new WaveInEvent
                {
                    DeviceNumber = 0, // Set the appropriate input device
                    WaveFormat = new WaveFormat(44100, 1) // 44.1 kHz, mono
                };

                string outputFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Recording.wav");
                waveFile = new WaveFileWriter(outputFilePath, waveIn.WaveFormat);

                waveIn.DataAvailable += WaveIn_DataAvailable;
                waveIn.RecordingStopped += WaveIn_RecordingStopped;

                // Start recording
                waveIn.StartRecording(); 

                // Reset and start the timer
                elapsedTime = TimeSpan.Zero;
                TimerDisplay.Text = "00:00:00";
                timer.Start();

                // Enable/disable buttons
                StartRecordingButton.IsEnabled = false;
                StopRecordingButton.IsEnabled = true;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting recording: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StopRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Stop the recording
                if (waveIn != null)
                {
                    waveIn.StopRecording();
                }

                // Stop the timer
                timer.Stop();

                // Enable/disable buttons
                StartRecordingButton.IsEnabled = true;
                StopRecordingButton.IsEnabled = false;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error stopping recording: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private async void ProcessButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Initialize tasks for asynchronous processing
                var tasks = new List<Task<string>>();
                // Disable the Clear All button
                ClearAllButton.IsEnabled = false;
                ProcessButton.IsEnabled = false;

                // Iterate through the files in the ListBox and start processing them
                foreach (var item in FilesListBox.Items)
                {
                    if (item is string filePath)
                    {
                        await GenerateNoteFromAudio(filePath);
                    }
                    await Task.Delay(20000);
                }
                // Disable the Clear All button
                ClearAllButton.IsEnabled = true;
                ProcessButton.IsEnabled = true;


                // Display a final summary message
                MessageBox.Show($"Processing completed for all files.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // Handle any errors during processing
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Re-enable the button
                ProcessButton.IsEnabled = true;
            }

        }

        private void ClearAllButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Clear all items from the FilesListBox
                FilesListBox.Items.Clear();
            }
            catch (Exception ex)
            {
                // Handle any errors during clearing
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}