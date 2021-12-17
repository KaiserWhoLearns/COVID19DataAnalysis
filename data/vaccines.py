import datetime
from collections import defaultdict

import numpy
import pandas
from pkg_resources import resource_filename


def __load_cdc_file(filepath=r'CDC Vaccination Data/vaccinations.csv'):
    local_ref_path = resource_filename(__name__, filepath)
    df = pandas.read_csv(local_ref_path)
    df[['Date']] = df[['Date']].apply(pandas.to_datetime)
    df['Date'] = df['Date'].dt.date
    return df


def generate_cdc_data_timeseries(df):
    dates = list(set(df['Date'].to_list()))
    dates.sort()
    # Structure: 'FIPS:Recip_County:Recip_State' : ['Series Complete Yes'...]
    unique_fips_codes = list(set(df['FIPS'].to_list()))
    print(len(unique_fips_codes), ' number of FIPS codes')

    columns = ['FIPS'] + dates

    rows = []
    for i, fips_code in enumerate(unique_fips_codes):
        if i % 100 == 0:
            print(f"Processed {i} FIPS entries")
        f_df = df[df['FIPS'] == fips_code]
        zero_count = [0] * len(dates)
        for index, row in f_df.iterrows():
            dt = row['Date']
            index = dates.index(dt)
            zero_count[index] = row['Series_Complete_Yes']
        r = [fips_code] + zero_count
        rows.append(r)

    t_df = pandas.DataFrame(data=rows, columns=columns)
    out_file = 'vaccine_timeseries_fully_vaccinated.csv'
    filepath = fr'CDC Vaccination Data/{out_file}.csv'
    local_ref_path = resource_filename(__name__, filepath)
    t_df.to_csv(local_ref_path, header=True, index=False)
    return t_df


def _compute_daily_from_aggregate_timeseries(agg_timeseries):
    daily_res = []
    yesterday = agg_timeseries[0]
    daily_res.append(yesterday)
    for today in agg_timeseries[1:]:
        ct = today - yesterday
        daily_res.append(ct)
        yesterday = today
    return daily_res


def generate_cdc_data_timeseries_daily(df=None, skip_columns_sequence=['FIPS']):
    if df is None:
        in_file = 'vaccine_timeseries_fully_vaccinated.csv'
        filepath = fr'CDC Vaccination Data/{in_file}'
        local_ref_path = resource_filename(__name__, filepath)
        df = pandas.read_csv(local_ref_path)
    rows = []
    columns = df.columns.values.tolist()
    for index, row in df.iterrows():
        r = [row['FIPS']]
        data = list(row)[len(skip_columns_sequence):]
        daily_data = _compute_daily_from_aggregate_timeseries(data)
        rows.append(r + daily_data)

    t_df = pandas.DataFrame(data=rows, columns=columns)
    out_file = 'daily_fully_vaccinated.csv'
    out_path = fr'CDC Vaccination Data/Timeseries/{out_file}'
    local_ref_path = resource_filename(__name__, out_path)
    t_df.to_csv(local_ref_path, header=True, index=False)
    return t_df


def get_cdc_data():
    return __load_cdc_file()


def __strip_county_borough_parish_ext(county_name, state_code):
    if state_code == "LA":
        return county_name.replace(" Parish", "")
    elif state_code == "AK":
        return county_name\
            .replace(" City and Borough", "")\
            .replace(" Census Area", "")\
            .replace(" Municipality", "")\
            .replace(" Borough", "")
    else:
        return county_name.replace(" County", "")


def generate_fips_mappings():
    df = get_cdc_data()
    unique_fips_codes = list(set(df['FIPS'].to_list()))
    d = defaultdict(list)
    rows = []
    columns = ['FIPS', 'County', 'State']
    for i, fips_code in enumerate(unique_fips_codes):
        if i % 100 == 0:
            print(f"Processing {i} FIPS entries")
        if fips_code not in d:
            f_df = df[df['FIPS'] == fips_code]
            county_name = list(set(f_df['Recip_County'].to_list()))
            state_name = list(set(f_df['Recip_State'].to_list()))
            # print(county_name, state_name, fips_code)
            if len(county_name) == len(state_name) and len(state_name) == 1:
                c_name = __strip_county_borough_parish_ext(county_name[0], state_name[0])
                d[fips_code] = [c_name, state_name[0]]
                rows.append([fips_code, c_name, state_name[0]])
        else:
            continue
    mapping_df = pandas.DataFrame(data=rows, columns=columns)
    out_file = 'mappings.csv'
    out_path = fr'CDC Vaccination Data/Timeseries/{out_file}'
    local_ref_path = resource_filename(__name__, out_path)
    mapping_df.to_csv(local_ref_path, header=True, index=False)
    return mapping_df


def merge_timeseries_mappings():
    in_file = 'daily_fully_vaccinated.csv'
    map_file = 'mappings.csv'
    filepath = fr'CDC Vaccination Data/Timeseries/{in_file}'
    map_path = fr'CDC Vaccination Data/Timeseries/{map_file}'
    local_f_ref_path = resource_filename(__name__, filepath)
    local_m_ref_path = resource_filename(__name__, map_path)
    df = pandas.read_csv(local_f_ref_path)
    m_df = pandas.read_csv(local_m_ref_path)
    r = pandas.merge(m_df, df, on='FIPS', how='left')
    r = r.replace(numpy.nan, 0)
    out_file = 'daily_timeseries_fully_vaccinated.csv'
    out_path = fr'CDC Vaccination Data/Timeseries/{out_file}'
    r.to_csv(resource_filename(__name__, out_path), header=True, index=False)
    return r


def read_timeseries_cdc_data():
    out_file = 'daily_timeseries_fully_vaccinated.csv'
    out_path = fr'CDC Vaccination Data/Timeseries/{out_file}'
    f_p = resource_filename(__name__, out_path)
    df = pandas.read_csv(f_p)
    return df


def get_case_to_vaccine_date_difference():
    case_start_date = datetime.datetime(year=2020, month=1, day=22)
    vaccine_start_date = datetime.datetime(year=2020, month=12, day=13)
    days_since_cases = case_start_date - vaccine_start_date
    return days_since_cases.days
