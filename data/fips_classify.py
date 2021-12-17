import pandas
from pkg_resources import resource_filename


__METROPOLITAN = [1.0, 2.0, 3.0]
__NON_METROPOLITAN = [4.0, 5.0, 6.0, 7.0, 8.0, 9.0]


def __load_file(filepath):
    local_ref_path = resource_filename(__name__, filepath)
    df = pandas.read_csv(local_ref_path)
    return df


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


def generate_fips_classification_rural_urban(out_file='county_classification.csv'):
    base_file_path = 'County Urban Rural/'
    file_name = 'usa_rural_urban.csv'
    file_path = f'{base_file_path}{file_name}'
    df = __load_file(file_path)
    df = df[df['FIPS'].notna()]
    columns = ['StateCode', 'FIPS', 'County', 'RU_Detail', 'M_or_NM']
    rows = []
    for index, r in df.iterrows():
        fips_code = r['FIPS']
        state_code = r['State']
        county_name = __strip_county_borough_parish_ext(r['County_Name'], state_code)
        ru_classification_detail = r['RUCC_2013']
        metro_or_non = 'M' if ru_classification_detail in __METROPOLITAN else 'NM'
        row = [state_code, fips_code, county_name, ru_classification_detail, metro_or_non]
        rows.append(row)
    f_df = pandas.DataFrame(data=rows, columns=columns)
    out_file_path = f'{base_file_path}{out_file}'
    out_f = resource_filename(__name__, out_file_path)
    f_df.to_csv(out_f, header=True, index=False)
    return f_df


def get_usa_counties_classification():
    base_file_path = 'County Urban Rural/'
    file_name = 'county_classification.csv'
    file_path = f'{base_file_path}{file_name}'
    df = __load_file(file_path)
    return df
