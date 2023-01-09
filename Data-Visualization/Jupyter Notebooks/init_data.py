import pandas as pd
pd.options.mode.chained_assignment = None
import csv
import numpy as np
from scipy import stats
import matplotlib.pyplot as plt

ids_A = ["A8", "A10", "A11", "A12", "A14", "A15", "A16", "A17", "A18"]
ids_B = ["B4", "B5", "B6", "B7", "B9", "B11", "B12", "B13", "B14"]

a_sample_dict, b_sample_dict, a_result_dict, b_result_dict = {}, {}, {}, {}

def init_results_and_samples():
    tmp = []

    for id in ids_A:
        for i in range(125):
            tmp.append(pd.read_csv(fr"C:\Users\kevin\Desktop\ExperimentResults\Group_A\userID_{id}\SampledMetrics\trialIteration_{i}_samples.csv", sep=";"))
            #tmp[i].dropna(axis='columns')
        a_sample_dict[id] = tmp
        tmp = []
        
        #fixing broken first row in results 
        result_path = fr'C:\Users\kevin\Desktop\ExperimentResults\Group_A\userID_{id}'
        with open(fr"{result_path}\Result.csv", 'rt') as inp, open(fr"{result_path}\Result_edit.csv", 'wt') as out:
            writer = csv.writer(out)
            for row in csv.reader(inp):
                if row != ['sep=;']:
                    writer.writerow(row)

        a_result_dict[id] = pd.read_csv(fr"{result_path}\Result_edit.csv", sep=";")
        # dropping irrelevant columns
        # a_result_dict[id].drop('TrialIteration', axis=1, inplace=True) # might be interesting, when sorting the columns after trialID
        a_result_dict[id].dropna(axis='columns', inplace=True)
        #a_result_dict[id] = a_result_dict[id].sort_values(by=['trialID'])
        a_result_dict[id]['trialID'] = pd.Series(a_result_dict[id]['trialID'], dtype="string").str.zfill(4)

    for id in ids_B:
        for i in range(125):
            tmp.append(pd.read_csv(fr"C:\Users\kevin\Desktop\ExperimentResults\Group_B\userID_{id}\SampledMetrics\trialIteration_{i}_samples.csv", sep=";"))
            tmp[i].dropna(axis='columns')
            tmp[i].drop(columns=['g_r', 'injected_rotations', 'additional_virtual_rotation_accumulation',
                            'virtual_position', 'virtual_direction', 'virtual_euler', 'virtual_rotation'],
                        inplace=True)
        b_sample_dict[id] = tmp
        tmp = []

        #fixing broken first row in results 
        result_path = fr'C:\Users\kevin\Desktop\ExperimentResults\Group_B\userID_{id}'
        with open(fr"{result_path}\Result.csv", 'rt') as inp, open(fr"{result_path}\Result_edit.csv", 'wt') as out:
            writer = csv.writer(out)
            for row in csv.reader(inp):
                if row != ['sep=;']:
                    writer.writerow(row)#
                    
        b_result_dict[id] = pd.read_csv(fr"{result_path}\Result_edit.csv", sep=";")
        # dropping irrelevant columns
        b_result_dict[id].drop(columns=['min_g_r', 'max_g_r', 'g_r_average', 'sum_injected_rotation_g_r(IN DEGREES)', 'injected_rotation_average',
                                        'real_dir_at_virt_yaw', 'virt_dir_at_virt_yaw', 'max_yaw_virt', 'real_pos_at_virt_yaw', 'virt_pos_at_virt_yaw',
                                        'real_euler_at_real_yaw','virt_euler_at_real_yaw', 'real_euler_at_virt_yaw', 'virt_euler_at_virt_yaw',
                                        'real_rot_at_real_yaw',	'virt_rot_at_real_yaw', 'real_rot_at_virt_yaw' ,'virt_rot_at_virt_yaw'],
                            inplace=True)
        # b_result_dict[id].drop('TrialIteration', axis=1, inplace=True)  # might be interesting, when sorting the columns after trialID
        b_result_dict[id].dropna(axis='columns', inplace=True)
        #b_result_dict[id] = b_result_dict[id].sort_values(by=['trialID'])
        b_result_dict[id]['trialID'] = pd.Series(b_result_dict[id]['trialID'], dtype="string").str.zfill(4)


def init_questionnaire():
    questionnaire_results = pd.read_csv(fr'C:\Users\kevin\Desktop\ExperimentResults\VR Ausweichstudie Fragebogen.csv')
    questionnaire_results.drop('Bitte geben Sie an, ob Sie die Datenschutz- und Einverständniserklärung gelesen haben und fragen Sie den Versuchsleiter, falls Ihnen diese nicht vorgelegt wurde, diese durchzusehen und zu unterzeichnen.', axis=1, inplace=True)
    questionnaire_results['ID Nummer'] = questionnaire_results['ID Nummer'].str.upper()
    # renaming IPQ columns:                 # general "sense of being there"
    questionnaire_results.rename(columns={'In der computererzeugten Welt hatte ich den Eindruck, dort gewesen zu sein... ': 'G1',
                                            # Spatial Presence
                                        'Ich hatte das Gefühl, daß die virtuelle Umgebung hinter mir weitergeht.': 'SP1',
                                        'Ich hatte das Gefühl, nur Bilder zu sehen. ': 'SP2',
                                        'Ich hatte nicht das Gefühl, in dem virtuellen Raum zu sein': "SP3",
                                        'Ich hatte das Gefühl, in dem virtuellen Raum zu handeln statt etwas von außen zu bedienen.': 'SP4',
                                        'Ich fühlte mich im virtuellen Raum anwesend. ': 'SP5',
                                            # Involvement 
                                        'Wie bewusst war Ihnen die reale Welt, während Sie sich durch die virtuelle Welt bewegten (z.B. Geräusche, Raumtemperatur, andere Personen etc.)? ':'INV1',
                                        'Meine reale Umgebung war mir nicht mehr bewusst.': 'INV2',
                                        'Ich achtete noch auf die reale Umgebung. ': 'INV3',
                                        'Meine Aufmerksamkeit war von der virtuellen Welt völlig in Bann gezogen. ':'INV4',
                                            # Experienced Realism
                                        'Wie real erschien Ihnen die virtuelle Umgebung? ': 'REAL1',
                                        'Wie sehr glich Ihr Erleben der virtuellen Umgebung dem Erleben einer realen Umgebung? ': "REAL2",
                                        'Wie real erschien Ihnen die virtuelle Welt? ': 'REAL3',
                                        'Die virtuelle Welt erschien mir wirklicher als die reale Welt. ': 'REAL4',                                      
                                        }, inplace=True)
    questionnaire_results['VRSQ-Score'] = [None] * len(questionnaire_results)
    questionnaire_results['SP-Score'] = [None] * len(questionnaire_results)
    questionnaire_results['INV-Score'] = [None] * len(questionnaire_results)
    questionnaire_results['REAL-Score'] = [None] * len(questionnaire_results)

    questionnaire_dict = {}
    for i in range(len(questionnaire_results)):
            questionnaire_dict[questionnaire_results.loc[i]['ID Nummer']] = questionnaire_results.loc[i]


def init_trial_ids():
    trial_id_list = []         # list of all trial IDs
    no_form_ids = set([])      # list of all trial IDs without form
    no_size_ids = set([])      # list of all trial IDs without size
    no_speed_ids = set([])     # list of all trial IDs without speed
    no_angle_ids = set([])     # list of all trial IDs without angle

    for i in range(len(a_result_dict['A10'])):
        trial_id = a_result_dict['A10'].iloc[i]['trialID']
        trial_id_list.append(trial_id)
        no_form_ids.add(trial_id[1:])
        no_size_ids.add(trial_id[:1] + trial_id[2:])
        no_speed_ids.add(trial_id[:2] + trial_id[3:])
        no_angle_ids.add(trial_id[:3])


class variable:
    name:str
    conditions:list[any]
    id_position:int

    def __init__(self, name:str, conditions:list[any], id_position:int):
        self.name = name
        self.conditions = conditions
        self.id_position = id_position

    def slice_trial_id(self, trial_id:str):
        sliced_id = trial_id[:self.id_position]+ trial_id[self.id_position+1:]
        return sliced_id


form_var = variable('form', ['Sphere', 'Cylinder', 'Car'], 0)
size_var = variable('size', [0.3, 0.8], 1)
speed_var = variable('speed', [18, 24, 30], 2)
angle_var = variable('angle', [40, 27, 13, 0, -13, -27, -40], 3)

def get_samples_for_key_by_variable(variable: variable, group_a_key:str, group_b_key=''):
    if group_b_key == '':
        group_b_key = group_a_key

    a_conditions = []
    b_conditions = []

    for condition in variable.conditions:
        a_samples = {}
        b_samples = {}

        for user_id in a_result_dict:
            condition_samples = a_result_dict[user_id].loc[a_result_dict[user_id][variable.name] == condition]

            for trial_id in condition_samples['trialID']:
                samples_by_trial = condition_samples.loc[condition_samples['trialID'] == trial_id][group_a_key].values[0]
                try:
                    a_samples[f'{variable.slice_trial_id(trial_id)}'].append(samples_by_trial)
                except KeyError:
                    a_samples[f'{variable.slice_trial_id(trial_id)}'] = []
                    a_samples[f'{variable.slice_trial_id(trial_id)}'].append(samples_by_trial)
        
        a_conditions.append(a_samples)
        
        
        for user_id in b_result_dict:
            condition_samples = b_result_dict[user_id].loc[b_result_dict[user_id][variable.name] == condition]

            for trial_id in condition_samples['trialID']:
                samples_by_trial = condition_samples.loc[condition_samples['trialID'] == trial_id][group_b_key].values[0]
                try:
                    b_samples[f'{variable.slice_trial_id(trial_id)}'].append(samples_by_trial)
                except KeyError:
                    b_samples[f'{variable.slice_trial_id(trial_id)}'] = []
                    b_samples[f'{variable.slice_trial_id(trial_id)}'].append(samples_by_trial)

        b_conditions.append(b_samples)
    
    return a_conditions, b_conditions