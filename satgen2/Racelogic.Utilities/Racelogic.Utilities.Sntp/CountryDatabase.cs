using System.Collections.Generic;

namespace Racelogic.Utilities.Sntp;

public static class CountryDatabase
{
	private static readonly Dictionary<string, Continent> continentByCountryName;

	static CountryDatabase()
	{
		continentByCountryName = new Dictionary<string, Continent>(250);
		continentByCountryName.Add("Andorra", Continent.Europe);
		continentByCountryName.Add("United Arab Emirates", Continent.Asia);
		continentByCountryName.Add("Afghanistan", Continent.Asia);
		continentByCountryName.Add("Antigua and Barbuda", Continent.NorthAmerica);
		continentByCountryName.Add("Anguilla", Continent.NorthAmerica);
		continentByCountryName.Add("Albania", Continent.Europe);
		continentByCountryName.Add("Armenia", Continent.Asia);
		continentByCountryName.Add("Angola", Continent.Africa);
		continentByCountryName.Add("Antarctica", Continent.Antarctica);
		continentByCountryName.Add("Argentina", Continent.SouthAmerica);
		continentByCountryName.Add("American Samoa", Continent.Oceania);
		continentByCountryName.Add("Austria", Continent.Europe);
		continentByCountryName.Add("Australia", Continent.Oceania);
		continentByCountryName.Add("Aruba", Continent.NorthAmerica);
		continentByCountryName.Add("Åland", Continent.Europe);
		continentByCountryName.Add("Azerbaijan", Continent.Asia);
		continentByCountryName.Add("Bosnia and Herzegovina", Continent.Europe);
		continentByCountryName.Add("Barbados", Continent.NorthAmerica);
		continentByCountryName.Add("Bangladesh", Continent.Asia);
		continentByCountryName.Add("Belgium", Continent.Europe);
		continentByCountryName.Add("Burkina Faso", Continent.Africa);
		continentByCountryName.Add("Bulgaria", Continent.Europe);
		continentByCountryName.Add("Bahrain", Continent.Asia);
		continentByCountryName.Add("Burundi", Continent.Africa);
		continentByCountryName.Add("Benin", Continent.Africa);
		continentByCountryName.Add("Saint Barthélemy", Continent.NorthAmerica);
		continentByCountryName.Add("Bermuda", Continent.NorthAmerica);
		continentByCountryName.Add("Brunei", Continent.Asia);
		continentByCountryName.Add("Bolivia", Continent.SouthAmerica);
		continentByCountryName.Add("Bonaire", Continent.NorthAmerica);
		continentByCountryName.Add("Brazil", Continent.SouthAmerica);
		continentByCountryName.Add("Bahamas", Continent.NorthAmerica);
		continentByCountryName.Add("Bhutan", Continent.Asia);
		continentByCountryName.Add("Bouvet Island", Continent.Antarctica);
		continentByCountryName.Add("Botswana", Continent.Africa);
		continentByCountryName.Add("Belarus", Continent.Europe);
		continentByCountryName.Add("Belize", Continent.NorthAmerica);
		continentByCountryName.Add("Canada", Continent.NorthAmerica);
		continentByCountryName.Add("Cocos [Keeling] Islands", Continent.Asia);
		continentByCountryName.Add("Democratic Republic of the Congo", Continent.Africa);
		continentByCountryName.Add("Central African Republic", Continent.Africa);
		continentByCountryName.Add("Republic of the Congo", Continent.Africa);
		continentByCountryName.Add("Switzerland", Continent.Europe);
		continentByCountryName.Add("Ivory Coast", Continent.Africa);
		continentByCountryName.Add("Cook Islands", Continent.Oceania);
		continentByCountryName.Add("Chile", Continent.SouthAmerica);
		continentByCountryName.Add("Cameroon", Continent.Africa);
		continentByCountryName.Add("China", Continent.Asia);
		continentByCountryName.Add("Colombia", Continent.SouthAmerica);
		continentByCountryName.Add("Costa Rica", Continent.NorthAmerica);
		continentByCountryName.Add("Cuba", Continent.NorthAmerica);
		continentByCountryName.Add("Cape Verde", Continent.Africa);
		continentByCountryName.Add("Curacao", Continent.NorthAmerica);
		continentByCountryName.Add("Christmas Island", Continent.Asia);
		continentByCountryName.Add("Cyprus", Continent.Europe);
		continentByCountryName.Add("Czech Republic", Continent.Europe);
		continentByCountryName.Add("Germany", Continent.Europe);
		continentByCountryName.Add("Djibouti", Continent.Africa);
		continentByCountryName.Add("Denmark", Continent.Europe);
		continentByCountryName.Add("Dominica", Continent.NorthAmerica);
		continentByCountryName.Add("Dominican Republic", Continent.NorthAmerica);
		continentByCountryName.Add("Algeria", Continent.Africa);
		continentByCountryName.Add("Ecuador", Continent.SouthAmerica);
		continentByCountryName.Add("Estonia", Continent.Europe);
		continentByCountryName.Add("Egypt", Continent.Africa);
		continentByCountryName.Add("Western Sahara", Continent.Africa);
		continentByCountryName.Add("Eritrea", Continent.Africa);
		continentByCountryName.Add("Spain", Continent.Europe);
		continentByCountryName.Add("Ethiopia", Continent.Africa);
		continentByCountryName.Add("Finland", Continent.Europe);
		continentByCountryName.Add("Fiji", Continent.Oceania);
		continentByCountryName.Add("Falkland Islands", Continent.SouthAmerica);
		continentByCountryName.Add("Micronesia", Continent.Oceania);
		continentByCountryName.Add("Faroe Islands", Continent.Europe);
		continentByCountryName.Add("France", Continent.Europe);
		continentByCountryName.Add("Gabon", Continent.Africa);
		continentByCountryName.Add("United Kingdom", Continent.Europe);
		continentByCountryName.Add("Grenada", Continent.NorthAmerica);
		continentByCountryName.Add("Georgia", Continent.Asia);
		continentByCountryName.Add("French Guiana", Continent.SouthAmerica);
		continentByCountryName.Add("Guernsey", Continent.Europe);
		continentByCountryName.Add("Ghana", Continent.Africa);
		continentByCountryName.Add("Gibraltar", Continent.Europe);
		continentByCountryName.Add("Greenland", Continent.NorthAmerica);
		continentByCountryName.Add("Gambia", Continent.Africa);
		continentByCountryName.Add("Guinea", Continent.Africa);
		continentByCountryName.Add("Guadeloupe", Continent.NorthAmerica);
		continentByCountryName.Add("Equatorial Guinea", Continent.Africa);
		continentByCountryName.Add("Greece", Continent.Europe);
		continentByCountryName.Add("South Georgia and the South Sandwich Islands", Continent.Antarctica);
		continentByCountryName.Add("Guatemala", Continent.NorthAmerica);
		continentByCountryName.Add("Guam", Continent.Oceania);
		continentByCountryName.Add("Guinea-Bissau", Continent.Africa);
		continentByCountryName.Add("Guyana", Continent.SouthAmerica);
		continentByCountryName.Add("Hong Kong", Continent.Asia);
		continentByCountryName.Add("Heard Island and McDonald Islands", Continent.Antarctica);
		continentByCountryName.Add("Honduras", Continent.NorthAmerica);
		continentByCountryName.Add("Croatia", Continent.Europe);
		continentByCountryName.Add("Haiti", Continent.NorthAmerica);
		continentByCountryName.Add("Hungary", Continent.Europe);
		continentByCountryName.Add("Indonesia", Continent.Asia);
		continentByCountryName.Add("Ireland", Continent.Europe);
		continentByCountryName.Add("Israel", Continent.Asia);
		continentByCountryName.Add("Isle of Man", Continent.Europe);
		continentByCountryName.Add("India", Continent.Asia);
		continentByCountryName.Add("British Indian Ocean Territory", Continent.Asia);
		continentByCountryName.Add("Iraq", Continent.Asia);
		continentByCountryName.Add("Iran", Continent.Asia);
		continentByCountryName.Add("Iceland", Continent.Europe);
		continentByCountryName.Add("Italy", Continent.Europe);
		continentByCountryName.Add("Jersey", Continent.Europe);
		continentByCountryName.Add("Jamaica", Continent.NorthAmerica);
		continentByCountryName.Add("Jordan", Continent.Asia);
		continentByCountryName.Add("Japan", Continent.Asia);
		continentByCountryName.Add("Kenya", Continent.Africa);
		continentByCountryName.Add("Kyrgyzstan", Continent.Asia);
		continentByCountryName.Add("Cambodia", Continent.Asia);
		continentByCountryName.Add("Kiribati", Continent.Oceania);
		continentByCountryName.Add("Comoros", Continent.Africa);
		continentByCountryName.Add("Saint Kitts and Nevis", Continent.NorthAmerica);
		continentByCountryName.Add("North Korea", Continent.Asia);
		continentByCountryName.Add("South Korea", Continent.Asia);
		continentByCountryName.Add("Kuwait", Continent.Asia);
		continentByCountryName.Add("Cayman Islands", Continent.NorthAmerica);
		continentByCountryName.Add("Kazakhstan", Continent.Asia);
		continentByCountryName.Add("Laos", Continent.Asia);
		continentByCountryName.Add("Lebanon", Continent.Asia);
		continentByCountryName.Add("Saint Lucia", Continent.NorthAmerica);
		continentByCountryName.Add("Liechtenstein", Continent.Europe);
		continentByCountryName.Add("Sri Lanka", Continent.Asia);
		continentByCountryName.Add("Liberia", Continent.Africa);
		continentByCountryName.Add("Lesotho", Continent.Africa);
		continentByCountryName.Add("Lithuania", Continent.Europe);
		continentByCountryName.Add("Luxembourg", Continent.Europe);
		continentByCountryName.Add("Latvia", Continent.Europe);
		continentByCountryName.Add("Libya", Continent.Africa);
		continentByCountryName.Add("Morocco", Continent.Africa);
		continentByCountryName.Add("Monaco", Continent.Europe);
		continentByCountryName.Add("Moldova", Continent.Europe);
		continentByCountryName.Add("Montenegro", Continent.Europe);
		continentByCountryName.Add("Saint Martin", Continent.NorthAmerica);
		continentByCountryName.Add("Madagascar", Continent.Africa);
		continentByCountryName.Add("Marshall Islands", Continent.Oceania);
		continentByCountryName.Add("Macedonia", Continent.Europe);
		continentByCountryName.Add("Mali", Continent.Africa);
		continentByCountryName.Add("Myanmar [Burma]", Continent.Asia);
		continentByCountryName.Add("Mongolia", Continent.Asia);
		continentByCountryName.Add("Macao", Continent.Asia);
		continentByCountryName.Add("Northern Mariana Islands", Continent.Oceania);
		continentByCountryName.Add("Martinique", Continent.NorthAmerica);
		continentByCountryName.Add("Mauritania", Continent.Africa);
		continentByCountryName.Add("Montserrat", Continent.NorthAmerica);
		continentByCountryName.Add("Malta", Continent.Europe);
		continentByCountryName.Add("Mauritius", Continent.Africa);
		continentByCountryName.Add("Maldives", Continent.Asia);
		continentByCountryName.Add("Malawi", Continent.Africa);
		continentByCountryName.Add("Mexico", Continent.NorthAmerica);
		continentByCountryName.Add("Malaysia", Continent.Asia);
		continentByCountryName.Add("Mozambique", Continent.Africa);
		continentByCountryName.Add("Namibia", Continent.Africa);
		continentByCountryName.Add("New Caledonia", Continent.Oceania);
		continentByCountryName.Add("Niger", Continent.Africa);
		continentByCountryName.Add("Norfolk Island", Continent.Oceania);
		continentByCountryName.Add("Nigeria", Continent.Africa);
		continentByCountryName.Add("Nicaragua", Continent.NorthAmerica);
		continentByCountryName.Add("Netherlands", Continent.Europe);
		continentByCountryName.Add("Norway", Continent.Europe);
		continentByCountryName.Add("Nepal", Continent.Asia);
		continentByCountryName.Add("Nauru", Continent.Oceania);
		continentByCountryName.Add("Niue", Continent.Oceania);
		continentByCountryName.Add("New Zealand", Continent.Oceania);
		continentByCountryName.Add("Oman", Continent.Asia);
		continentByCountryName.Add("Panama", Continent.NorthAmerica);
		continentByCountryName.Add("Peru", Continent.SouthAmerica);
		continentByCountryName.Add("French Polynesia", Continent.Oceania);
		continentByCountryName.Add("Papua New Guinea", Continent.Oceania);
		continentByCountryName.Add("Philippines", Continent.Asia);
		continentByCountryName.Add("Pakistan", Continent.Asia);
		continentByCountryName.Add("Poland", Continent.Europe);
		continentByCountryName.Add("Saint Pierre and Miquelon", Continent.NorthAmerica);
		continentByCountryName.Add("Pitcairn Islands", Continent.Oceania);
		continentByCountryName.Add("Puerto Rico", Continent.NorthAmerica);
		continentByCountryName.Add("Palestine", Continent.Asia);
		continentByCountryName.Add("Portugal", Continent.Europe);
		continentByCountryName.Add("Palau", Continent.Oceania);
		continentByCountryName.Add("Paraguay", Continent.SouthAmerica);
		continentByCountryName.Add("Qatar", Continent.Asia);
		continentByCountryName.Add("Réunion", Continent.Africa);
		continentByCountryName.Add("Romania", Continent.Europe);
		continentByCountryName.Add("Serbia", Continent.Europe);
		continentByCountryName.Add("Russia", Continent.Europe);
		continentByCountryName.Add("Rwanda", Continent.Africa);
		continentByCountryName.Add("Saudi Arabia", Continent.Asia);
		continentByCountryName.Add("Solomon Islands", Continent.Oceania);
		continentByCountryName.Add("Seychelles", Continent.Africa);
		continentByCountryName.Add("Sudan", Continent.Africa);
		continentByCountryName.Add("Sweden", Continent.Europe);
		continentByCountryName.Add("Singapore", Continent.Asia);
		continentByCountryName.Add("Saint Helena", Continent.Africa);
		continentByCountryName.Add("Slovenia", Continent.Europe);
		continentByCountryName.Add("Svalbard and Jan Mayen", Continent.Europe);
		continentByCountryName.Add("Slovakia", Continent.Europe);
		continentByCountryName.Add("Sierra Leone", Continent.Africa);
		continentByCountryName.Add("San Marino", Continent.Europe);
		continentByCountryName.Add("Senegal", Continent.Africa);
		continentByCountryName.Add("Somalia", Continent.Africa);
		continentByCountryName.Add("Suriname", Continent.SouthAmerica);
		continentByCountryName.Add("South Sudan", Continent.Africa);
		continentByCountryName.Add("São Tomé and Príncipe", Continent.Africa);
		continentByCountryName.Add("El Salvador", Continent.NorthAmerica);
		continentByCountryName.Add("Sint Maarten", Continent.NorthAmerica);
		continentByCountryName.Add("Syria", Continent.Asia);
		continentByCountryName.Add("Swaziland", Continent.Africa);
		continentByCountryName.Add("Turks and Caicos Islands", Continent.NorthAmerica);
		continentByCountryName.Add("Chad", Continent.Africa);
		continentByCountryName.Add("French Southern Territories", Continent.Antarctica);
		continentByCountryName.Add("Togo", Continent.Africa);
		continentByCountryName.Add("Thailand", Continent.Asia);
		continentByCountryName.Add("Tajikistan", Continent.Asia);
		continentByCountryName.Add("Tokelau", Continent.Oceania);
		continentByCountryName.Add("East Timor", Continent.Oceania);
		continentByCountryName.Add("Turkmenistan", Continent.Asia);
		continentByCountryName.Add("Tunisia", Continent.Africa);
		continentByCountryName.Add("Tonga", Continent.Oceania);
		continentByCountryName.Add("Turkey", Continent.Asia);
		continentByCountryName.Add("Trinidad and Tobago", Continent.NorthAmerica);
		continentByCountryName.Add("Tuvalu", Continent.Oceania);
		continentByCountryName.Add("Taiwan", Continent.Asia);
		continentByCountryName.Add("Tanzania", Continent.Africa);
		continentByCountryName.Add("Ukraine", Continent.Europe);
		continentByCountryName.Add("Uganda", Continent.Africa);
		continentByCountryName.Add("U.S. Minor Outlying Islands", Continent.Oceania);
		continentByCountryName.Add("United States", Continent.NorthAmerica);
		continentByCountryName.Add("Uruguay", Continent.SouthAmerica);
		continentByCountryName.Add("Uzbekistan", Continent.Asia);
		continentByCountryName.Add("Vatican City", Continent.Europe);
		continentByCountryName.Add("Saint Vincent and the Grenadines", Continent.NorthAmerica);
		continentByCountryName.Add("Venezuela", Continent.SouthAmerica);
		continentByCountryName.Add("British Virgin Islands", Continent.NorthAmerica);
		continentByCountryName.Add("U.S. Virgin Islands", Continent.NorthAmerica);
		continentByCountryName.Add("Vietnam", Continent.Asia);
		continentByCountryName.Add("Vanuatu", Continent.Oceania);
		continentByCountryName.Add("Wallis and Futuna", Continent.Oceania);
		continentByCountryName.Add("Samoa", Continent.Oceania);
		continentByCountryName.Add("Kosovo", Continent.Europe);
		continentByCountryName.Add("Yemen", Continent.Asia);
		continentByCountryName.Add("Mayotte", Continent.Africa);
		continentByCountryName.Add("South Africa", Continent.Africa);
		continentByCountryName.Add("Zambia", Continent.Africa);
		continentByCountryName.Add("Zimbabwe", Continent.Africa);
	}

	public static Continent GetContinent(string countryName)
	{
		countryName = countryName.ToLowerInvariant();
		foreach (KeyValuePair<string, Continent> item in continentByCountryName)
		{
			if (item.Key.ToLowerInvariant() == countryName)
			{
				return item.Value;
			}
		}
		return Continent.Europe;
	}
}